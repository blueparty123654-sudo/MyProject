using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class Order
    {
        [Key] // 👈 ติดป้าย
        public int OrderId { get; set; } // เดิมคือ OdId
        public int UserId { get; set; }
        public int ProductId { get; set; } // เดิมคือ PrId
        public int? DiscountId { get; set; }

        public DateOnly DateReceipt { get; set; } // เดิมคือ OdDateReceipt
        public DateOnly DateReturn { get; set; } // เดิมคือ OdDateReturn
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; } // เดิมคือ OdPrice
        public int Point { get; set; } // เดิมคือ OdPoint

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Payment? Payment { get; set; }
        public virtual Discount? Discount { get; set; }
    }
}