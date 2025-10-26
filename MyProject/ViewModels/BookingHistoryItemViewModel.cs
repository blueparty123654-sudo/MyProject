// ใน ViewModels/BookingHistoryItemViewModel.cs
using System;

namespace MyProject.ViewModels
{
    public class BookingHistoryItemViewModel
    {
        public int OrderId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public DateOnly PickupDate { get; set; }
        public DateOnly ReturnDate { get; set; }
        public string? Status { get; set; } // เช่น "เสร็จสิ้น", "กำลังดำเนินการ", "ยกเลิก" (Optional)
        public decimal FinalPrice { get; set; } // (Optional) อาจจะแสดงราคา
    }
}