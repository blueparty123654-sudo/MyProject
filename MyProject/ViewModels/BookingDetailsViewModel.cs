// ใน ViewModels/BookingDetailsViewModel.cs
using System;
namespace MyProject.ViewModels
{
    public class BookingDetailsViewModel
    {
        public int OrderId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? BranchName { get; set; }
        public DateOnly PickupDate { get; set; }
        public DateOnly ReturnDate { get; set; }
        public int NumberOfDays { get; set; }
        public decimal OriginalPrice { get; set; }
        public string? DiscountCode { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public int PointsEarned { get; set; }
        public string? UserName { get; set; }
    }
}