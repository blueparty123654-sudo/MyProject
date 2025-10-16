using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class BranchProduct
    {
        // Composite Primary Key (กุญแจหลักแบบผสม)
        public int BranchId { get; set; }
        public int ProductId { get; set; }

        public int StockQuantity { get; set; } // จำนวนรถรุ่นนี้ที่มีในสาขานี้

        // Navigation Properties
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}