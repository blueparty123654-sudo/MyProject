using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class BranchProduct
    {
        // Composite Primary Key (กุญแจหลักแบบผสม)
        public int BranchId { get; set; }
        public int ProductId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must not be negative")]
        public int StockQuantity { get; set; }

        // Navigation Properties
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}