// ใน ViewModels/RentalHistoryItemViewModel.cs
using System;

namespace MyProject.ViewModels
{
    public class RentalHistoryItemViewModel
    {
        public int OrderId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public DateOnly PickupDate { get; set; }
        public DateOnly ReturnDate { get; set; }
        public DateTime PaymentDate { get; set; } // วันที่ชำระเงิน
        public string PaymentStatus { get; set; } = null!; // สถานะจาก Payment ("In progress", "Completed")
        public decimal AmountPaid { get; set; } // ยอดที่จ่าย
    }
}