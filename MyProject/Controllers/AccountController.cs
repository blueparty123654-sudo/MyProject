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

        // --- Action สำหรับการสมัครสมาชิก ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 1. ตรวจสอบว่าอีเมลนี้ถูกใช้งานแล้วหรือยัง (Server-side validation)
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "อีเมลนี้ถูกใช้งานแล้ว");
            }

            // 2. ตรวจสอบ Validation ทั้งหมด (รวมถึงข้อ 1)
            if (!ModelState.IsValid)
            {
                // ส่งรายการ Error ทั้งหมดกลับไปให้ AJAX จัดการ
                return Json(new { success = false, errors = ModelStateToDictionary() });
            }

            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            if (customerRole == null)
            {
                return Json(new { success = false, message = "เกิดข้อผิดพลาด: ไม่พบบทบาทเริ่มต้นสำหรับผู้ใช้" });
            }

            // 3. จัดการการอัปโหลดไฟล์
            string uniqueFileName = await UploadFileAsync(model.DrivingLicenseFile, "driving_licenses");

            var hashedPassword = HashPassword(model.Password);
            var birthDate = DateOnly.ParseExact(model.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var user = new User
            {
                Name = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = birthDate,
                PasswordHash = hashedPassword,
                DrivingLicenseImageUrl = uniqueFileName,
                RoleId = customerRole.RoleId,
                Status = "รอการตรวจสอบ"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "สมัครสมาชิกสำเร็จ! กำลังพาคุณไปยังหน้าหลัก...", redirectUrl = Url.Action("Index", "Home") });
        }

        // --- Action สำหรับการเข้าสู่ระบบ ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "กรุณากรอกข้อมูลให้ครบถ้วน" });
            }

            var user = await _context.Users
                                     .Include(u => u.Role)
                                     .FirstOrDefaultAsync(u => u.Email == model.Email);

            // ตรวจสอบทั้ง user และ password ในคราวเดียว
            if (user == null || user.PasswordHash != HashPassword(model.Password))
            {
                return Json(new { success = false, message = "อีเมลหรือรหัสผ่านไม่ถูกต้อง" });
            }

            // สร้าง Claims สำหรับเก็บข้อมูลในคุกกี้
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Points", user.UserPoint.ToString()),
                new Claim("Status", user.Status ?? "N/A"),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Customer")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

            return Json(new { success = true, message = "เข้าสู่ระบบสำเร็จ! กำลังโหลดข้อมูลของคุณ...", redirectUrl = Url.Action("Index", "Home") });
        }

        // --- Action สำหรับการออกจากระบบ ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // --- Action สำหรับดึงข้อมูลโปรไฟล์ (สำหรับ AJAX) ---
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null) return Unauthorized();

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return NotFound();

            return Json(new
            {
                userName = user.Name,
                email = user.Email,
                dateOfBirth = user.DateOfBirth.ToString("yyyy-MM-dd")
            });
        }

        // --- Action สำหรับอัปเดตโปรไฟล์ ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            if (user == null) return Json(new { success = false, message = "ไม่พบผู้ใช้ในระบบ" });

            // ตรวจสอบ Validation ต่างๆ แล้วเพิ่ม Error เข้าไปใน ModelState
            if (user.Email != model.Email && await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "อีเมลใหม่นี้ถูกใช้งานโดยบัญชีอื่นแล้ว");
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword) || user.PasswordHash != HashPassword(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "รหัสผ่านปัจจุบันไม่ถูกต้อง");
                }
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelStateToDictionary() });
            }

            // อัปเดตข้อมูล
            user.Name = model.UserName;
            user.Email = model.Email;
            user.DateOfBirth = DateOnly.ParseExact(model.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.PasswordHash = HashPassword(model.NewPassword);
            }

            if (model.NewDrivingLicenseFile != null)
            {
                // (ถ้ามี) ลบไฟล์เก่าก่อนอัปโหลดไฟล์ใหม่
                if (!string.IsNullOrEmpty(user.DrivingLicenseImageUrl))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/driving_licenses", user.DrivingLicenseImageUrl);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                user.DrivingLicenseImageUrl = await UploadFileAsync(model.NewDrivingLicenseFile, "driving_licenses");
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // อัปเดตคุกกี้เพื่อให้ข้อมูลบน Navbar ถูกต้องทันที
            await UpdateUserClaims(user);

            return Json(new { success = true, message = "อัปเดตโปรไฟล์สำเร็จ! ข้อมูลของคุณถูกบันทึกแล้ว", redirectUrl = Url.Action("Index", "Home") });
        }


        // --- Helper Methods (ฟังก์ชันช่วย) ---

        // ฟังก์ชันสำหรับ Hash รหัสผ่าน
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // ฟังก์ชันสำหรับอัปโหลดไฟล์
        private async Task<string> UploadFileAsync(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0) return string.Empty;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // คืนค่าเฉพาะชื่อไฟล์เพื่อเก็บในฐานข้อมูล
            return uniqueFileName;
        }

        // ฟังก์ชันสำหรับแปลง ModelState เป็น Dictionary เพื่อส่งกลับเป็น JSON
        private Dictionary<string, string[]> ModelStateToDictionary()
        {
            return ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
        }

        // ฟังก์ชันสำหรับอัปเดต Claims ในคุกกี้
        private async Task UpdateUserClaims(User user)
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            var newClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Points", user.UserPoint.ToString()),
                new Claim("Status", user.Status ?? "N/A"),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Customer")
            };
            var claimsIdentity = new ClaimsIdentity(newClaims, "MyCookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
        }
    }
}