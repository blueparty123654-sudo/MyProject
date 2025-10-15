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

        // ... (Action Register, Login, Logout เหมือนเดิม) ...

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

            // ... (ส่วนอัปเดต UserName, Email, UserDob, และรหัสผ่าน เหมือนเดิม) ...
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

            // *** ส่วนที่แก้ไข: เปลี่ยนเป็นการอัปเดตใบขับขี่ ***
            if (model.NewDrivingLicenseFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/driving_licenses"); // ใช้โฟลเดอร์เดิม
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // (Optional but recommended) ลบไฟล์เก่าทิ้งเพื่อประหยัดพื้นที่
                if (!string.IsNullOrEmpty(user.UserDrivingcard))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, user.UserDrivingcard);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // บันทึกไฟล์ใหม่
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.NewDrivingLicenseFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.NewDrivingLicenseFile.CopyToAsync(fileStream);
                }
                user.UserDrivingcard = uniqueFileName; // << บันทึกชื่อไฟล์ลงคอลัมน์ที่ถูกต้อง
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // ... (ส่วน SignOut/SignIn เพื่ออัปเดต Cookie เหมือนเดิม) ...
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

        // ... (ฟังก์ชัน HashPassword เหมือนเดิม) ...
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}