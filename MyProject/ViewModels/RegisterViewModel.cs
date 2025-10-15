using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MyProject.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
        [StringLength(100, ErrorMessage = "ชื่อต้องมีความยาวไม่เกิน 100 ตัวอักษร")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        // ใช้ Regular Expression เพื่อบังคับให้เป็นตัวเลข 9-10 หลัก
        [RegularExpression(@"^0[0-9]{8,9}$", ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกวันเกิด")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 8 ตัวอักษร")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // เพิ่มช่องยืนยันรหัสผ่าน
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "รหัสผ่านและการยืนยันรหัสผ่านไม่ตรงกัน")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาอัปโหลดรูปใบขับขี่")]
        public IFormFile DrivingLicenseFile { get; set; } = null!;
    }
}