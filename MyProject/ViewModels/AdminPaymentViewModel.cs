// ใน ViewModels/AdminPaymentViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    public class AdminPaymentViewModel
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string UserName { get; set; } = string.Empty; // ชื่อลูกค้า
        public string ProductName { get; set; } = string.Empty; // (Optional) ชื่อสินค้า
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;

        // สำหรับ Dropdown หรือ Input เพื่อเปลี่ยน Status
        [Required(ErrorMessage = "กรุณาเลือกสถานะใหม่")]
        public string NewStatus { get; set; } = string.Empty;

        // ตัวเลือก Status ที่เป็นไปได้ (สำหรับ Dropdown)
        public static List<string> AvailableStatuses { get; } = new List<string> { "In progress", "Completed", "Failed", "Refunded" }; // เพิ่ม/แก้ไขตามต้องการ
    }
}