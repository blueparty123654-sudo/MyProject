using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Method { get; set; } = null!;

        [Required]
        public string Status { get; set; } = "Pending"; // e.g., "Pending", "Completed", "Failed", "Refunded"

        // Foreign Key to Order
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}