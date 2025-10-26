using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    public class ProductDetailsViewModel
    {
        // --- Product Info ---
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        // public string? Description { get; set; } // ไม่มีในตาราง Products
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageUrl2 { get; set; }

        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        // --- Specifications (ใช้ชื่อเดียวกับ Product.cs ที่แก้แล้ว) ---
        public string? GearType { get; set; }
        public string? Engine { get; set; }
        public string? CoolingSystem { get; set; }
        public string? StartingSystem { get; set; } // Starting ไม่ใช่ Starter
        public string? FuelType { get; set; }
        public string? FuelDispensing { get; set; } // Dispensing ไม่ใช่ System
        public string? FuelTankCapacity { get; set; } // TankCapacity
        public string? BrakeSystem { get; set; }
        public string? Suspension { get; set; }
        public string? TireSize { get; set; } // TireSize
        public string? Dimensions { get; set; }
        public string? VehicleWeight { get; set; } // VehicleWeight

        // --- Booking Inputs ---
        [DataType(DataType.Date)]
        [Display(Name = "วันที่รับรถ")]
        public DateTime? PickupDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "วันที่คืนรถ")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "รหัสส่วนลด (ถ้ามี)")]
        public string? DiscountCode { get; set; }
    }
}