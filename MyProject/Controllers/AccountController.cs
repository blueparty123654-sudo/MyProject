using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyBookstoreDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(MyBookstoreDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                return Json(new { success = false, message = firstError ?? "ข้อมูลไม่ถูกต้อง" });
            }

            if (await _context.Users.AnyAsync(u => u.UserEmail == model.Email))
            {
                return Json(new { success = false, message = "อีเมลนี้ถูกใช้งานแล้ว" });
            }

            string uniqueFileName = "";
            if (model.DrivingLicenseFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/driving_licenses");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.DrivingLicenseFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.DrivingLicenseFile.CopyToAsync(fileStream);
                }
            }

            var hashedPassword = HashPassword(model.Password);
            var birthDate = DateOnly.ParseExact(model.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var user = new User
            {
                UserName = model.UserName,
                UserEmail = model.Email,
                UserNo = model.PhoneNumber,
                UserDob = birthDate,
                UserPass = hashedPassword,
                UserDrivingcard = uniqueFileName,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["RegisterSuccess"] = "สมัครสมาชิกสำเร็จ!";
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "กรุณากรอกข้อมูลให้ครบถ้วน" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == model.Email);

            if (user == null || user.UserPass != HashPassword(model.Password))
            {
                return Json(new { success = false, message = "อีเมลหรือรหัสผ่านไม่ถูกต้อง" });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.UserEmail),
                new Claim("Points", user.UserPoint.ToString()),
                new Claim("Status", user.Status ?? "N/A"),
            };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

            return Json(new { success = true });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // ===================================
        // ==   ACTION: GET PROFILE DATA    ==
        // ===================================
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            if (user == null) return NotFound();

            return Json(new
            {
                userName = user.UserName,
                email = user.UserEmail,
                dateOfBirth = user.UserDob.ToString("yyyy-MM-dd")
            });
        }
        // ===================================
        // ==   ACTION: UPDATE PROFILE      ==
        // ===================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                return Json(new { success = false, message = firstError ?? "ข้อมูลไม่ถูกต้อง" });
            }

            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == currentUserEmail);
            if (user == null) return Json(new { success = false, message = "ไม่พบผู้ใช้ในระบบ" });

            if (user.UserEmail != model.Email && await _context.Users.AnyAsync(u => u.UserEmail == model.Email))
            {
                return Json(new { success = false, message = "อีเมลใหม่นี้ถูกใช้งานโดยบัญชีอื่นแล้ว" });
            }

            user.UserName = model.UserName;
            user.UserEmail = model.Email;
            user.UserDob = DateOnly.ParseExact(model.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword) || user.UserPass != HashPassword(model.CurrentPassword))
                {
                    return Json(new { success = false, message = "รหัสผ่านปัจจุบันไม่ถูกต้อง" });
                }
                user.UserPass = HashPassword(model.NewPassword);
            }

            if (model.NewDrivingLicenseFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/driving_licenses");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(user.UserDrivingcard))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, user.UserDrivingcard);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.NewDrivingLicenseFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.NewDrivingLicenseFile.CopyToAsync(fileStream);
                }
                user.UserDrivingcard = uniqueFileName;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync("MyCookieAuth");
            var newClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.UserEmail),
        new Claim("Points", user.UserPoint.ToString()),
        new Claim("Status", user.Status ?? "N/A"),
    };
            var claimsIdentity = new ClaimsIdentity(newClaims, "MyCookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

            return Json(new { success = true, message = "อัปเดตโปรไฟล์สำเร็จ!" });
        }
    }
}