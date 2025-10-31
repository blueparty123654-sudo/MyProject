// ใน ViewModels/PaymentViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    public class PaymentViewModel
    {
        [Required]
        public int OrderId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public DateOnly PickupDate { get; set; }
        public DateOnly ReturnDate { get; set; }
        public decimal FinalPrice { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกวิธีการชำระเงิน")]
        public string? SelectedPaymentMethod { get; set; }

        // --- Card Details ---
        [Display(Name = "ชื่อบนบัตร")]
        public string? CardHolderName { get; set; } // <-- เพิ่ม

        [Display(Name = "หมายเลขบัตร")]
        public string? CardNumber { get; set; }

        [Display(Name = "วันหมดอายุ (MM/YY)")]
        public string? ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        public string? Cvv { get; set; }
    }
}