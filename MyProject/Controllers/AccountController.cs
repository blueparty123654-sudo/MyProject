using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System.Globalization;
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
            // *** การเปลี่ยนแปลงที่สำคัญที่สุด: ตรวจสอบ ModelState ก่อนเป็นอันดับแรก ***
            if (!ModelState.IsValid)
            {
                // ค้นหาข้อความ Error แรกที่เจอใน ModelState เพื่อนำมาแสดง
                var firstError = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();

                // นำข้อความ Error ที่เจอมาใส่ใน TempData ถ้าไม่เจอก็ใช้ข้อความกลางๆ แทน
                TempData["RegisterError"] = firstError ?? "ข้อมูลไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง";

                return RedirectToAction("Index", "Home");
            }

            // --- โค้ดส่วนที่เหลือจะทำงานก็ต่อเมื่อข้อมูลเบื้องต้นถูกต้องทั้งหมดแล้ว ---

            // 1. ตรวจสอบอีเมลซ้ำ
            if (await _context.Users.AnyAsync(u => u.UserEmail == model.Email))
            {
                TempData["RegisterError"] = "อีเมลนี้ถูกใช้งานแล้ว";
                return RedirectToAction("Index", "Home");
            }

            // 2. จัดการการอัปโหลดไฟล์
            string uniqueFileName = "";
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

            // 4. แปลงวันที่ (ตอนนี้มั่นใจได้ว่า Format ถูกต้องเพราะผ่าน ModelState มาแล้ว)
            var birthDate = DateOnly.ParseExact(model.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            // 5. สร้าง Object User ใหม่
            var user = new User
            {
                UserName = model.UserName,
                UserEmail = model.Email,
                UserNo = model.PhoneNumber,
                UserDob = birthDate,
                UserPass = hashedPassword,
                UserDrivingcard = uniqueFileName,
            };

            // 6. บันทึกข้อมูล
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["RegisterSuccess"] = "สมัครสมาชิกสำเร็จ!";
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