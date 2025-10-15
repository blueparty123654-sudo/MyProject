using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace MyProject.Controllers // ตรวจสอบว่า namespace ตรงกับโปรเจกต์ของคุณ
{
    public class AccountController : Controller // << บรรทัดนี้สำคัญที่สุด! ต้องมี ": Controller"
    {
        private readonly MyBookstoreDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor สำหรับรับ DbContext และ IWebHostEnvironment
        public AccountController(MyBookstoreDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. ตรวจสอบอีเมลซ้ำ
                if (await _context.Users.AnyAsync(u => u.UserEmail == model.Email))
                {
                    TempData["RegisterError"] = "อีเมลนี้ถูกใช้งานแล้ว";
                    return RedirectToAction("Index", "Home");
                }

                // 2. จัดการการอัปโหลดไฟล์
                string uniqueFileName = ""; // กำหนดค่าเริ่มต้น
                if (model.DrivingLicenseFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/driving_licenses");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.DrivingLicenseFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.DrivingLicenseFile.CopyToAsync(fileStream);
                    }
                }

                // 3. Hash รหัสผ่าน
                var hashedPassword = HashPassword(model.Password);

                // 4. สร้าง Object User ใหม่
                var user = new User
                {
                    UserName = model.UserName,
                    UserEmail = model.Email,
                    UserNo = model.PhoneNumber,
                    UserDob = DateOnly.FromDateTime(model.DateOfBirth),
                    UserPass = hashedPassword,
                    UserDrivingcard = uniqueFileName,
                };

                // 5. บันทึกข้อมูล
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["RegisterSuccess"] = "สมัครสมาชิกสำเร็จ!";
                return RedirectToAction("Index", "Home");
            }

            TempData["RegisterError"] = "ข้อมูลไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง";
            return RedirectToAction("Index", "Home");
        }

        // ฟังก์ชันสำหรับ Hash Password ต้องอยู่ภายใน Class
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