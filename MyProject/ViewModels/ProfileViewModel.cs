using MyProject.Validation;
using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    public class ProfileViewModel
    {
        // --- ส่วนข้อมูลโปรไฟล์ ---
        [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกวันเกิด")]
        [AgeRange(16, 100, ErrorMessage = "อายุต้องอยู่ระหว่าง 16 ถึง 100 ปี")]
        public string DateOfBirth { get; set; } = string.Empty;

        // สำหรับอัปโหลดใบขับขี่ใหม่ (ไม่บังคับ)
        public IFormFile? NewDrivingLicenseFile { get; set; }


        // --- ส่วนเปลี่ยนรหัสผ่าน (ไม่บังคับ) ---
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "รหัสผ่านใหม่ต้องมีความยาวอย่างน้อย 8 ตัวอักษร")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "รหัสผ่านใหม่และการยืนยันไม่ตรงกัน")]
        public string? ConfirmNewPassword { get; set; }
    }
}