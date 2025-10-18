using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    // ใช้แสดงข้อมูลในหน้า Product/Details.cshtml
    public class ProductDetailsViewModel
    {
        // --- Product Info ---
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } // *** ตรวจสอบชื่อคอลัมน์ Description ***
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageUrl2 { get; set; }

        // --- Specifications (ใช้ชื่อให้ตรงกับ Model Product.cs และตาราง Products) ---
        // *** ตรวจสอบชื่อ Property/คอลัมน์เหล่านี้ทั้งหมด ***
        public string? ModelYear { get; set; }
        public string? Gear { get; set; }
        public string? Engine { get; set; }
        public string? Cooling { get; set; }
        public string? Starter { get; set; }
        public string? CC { get; set; }
        public string? Fuel { get; set; }
        public string? FuelSystem { get; set; }
        public string? FuelCapacity { get; set; }
        public string? Suspension { get; set; }
        public string? Brake { get; set; }
        public string? Wheels { get; set; }
        public string? Tires { get; set; }
        public string? Dimensions { get; set; }
        public string? Weight { get; set; }
        // เพิ่ม Property อื่นๆ ถ้ามี...

        // --- Booking Inputs ---
        [DataType(DataType.Date)]
        [Display(Name = "วันที่รับรถ")]
        public DateTime? PickupDate { get; set; } // ใช้ DateTime? (nullable) สำหรับฟอร์มเริ่มต้น

        [DataType(DataType.Date)]
        [Display(Name = "วันที่คืนรถ")]
        public DateTime? ReturnDate { get; set; } // ใช้ DateTime? (nullable) สำหรับฟอร์มเริ่มต้น

        [Display(Name = "รหัสส่วนลด (ถ้ามี)")]
        public string? DiscountCode { get; set; }
    }
}