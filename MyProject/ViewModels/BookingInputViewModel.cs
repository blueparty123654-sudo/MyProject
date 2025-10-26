using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    // ใช้รับข้อมูลจากฟอร์มจองในหน้า Details ส่งไป BookingController
    public class BookingInputViewModel
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกวันที่รับรถ")]
        [DataType(DataType.Date)]
        [Display(Name = "วันที่รับรถ")]
        public DateTime PickupDate { get; set; } // ใช้ DateTime (non-nullable) เพราะ Required

        [Required(ErrorMessage = "กรุณาเลือกวันที่คืนรถ")]
        [DataType(DataType.Date)]
        [Display(Name = "วันที่คืนรถ")]
        public DateTime ReturnDate { get; set; } // ใช้ DateTime (non-nullable) เพราะ Required

        [Display(Name = "รหัสส่วนลด")]
        public string? DiscountCode { get; set; }

        // public int BranchId { get; set; } // Uncomment ถ้าจะส่ง BranchId มาจากฟอร์ม
    }
}