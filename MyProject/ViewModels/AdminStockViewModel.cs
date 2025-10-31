// ใน ViewModels/AdminStockViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    public class AdminStockViewModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty; // (Optional) รูปสินค้า
        public int CurrentStock { get; set; }

        // สำหรับ Input เพื่อเพิ่ม Stock
        [Range(0, 1000, ErrorMessage = "จำนวนที่เพิ่มต้องอยู่ระหว่าง 0 ถึง 1000")] // กำหนด Range ตามความเหมาะสม
        public int QuantityToAdd { get; set; } = 0; // ค่าเริ่มต้นเป็น 0
    }
}