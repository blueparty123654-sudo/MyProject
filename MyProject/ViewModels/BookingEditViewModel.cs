// ใน ViewModels/BookingEditViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    public class BookingEditViewModel
    {
        [Required]
        public int OrderId { get; set; } // ID ของ Order ที่จะแก้

        // ข้อมูลที่แสดงเฉยๆ
        public string? ProductName { get; set; }
        public string? BranchName { get; set; }

        [Required]
        public int ProductId { get; set; } // ต้องส่งกลับไป Controller

        // ข้อมูลที่แก้ไขได้
        [Required(ErrorMessage = "กรุณาเลือกวันที่รับรถ")]
        [DataType(DataType.Date)]
        [Display(Name = "วันที่รับรถ")]
        public DateTime PickupDate { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกวันที่คืนรถ")]
        [DataType(DataType.Date)]
        [Display(Name = "วันที่คืนรถ")]
        public DateTime ReturnDate { get; set; }

        [Display(Name = "รหัสส่วนลด (ถ้ามี)")]
        public string? DiscountCode { get; set; }

        // (Optional) อาจเพิ่ม Properties อื่นๆ ที่จำเป็น
    }
}