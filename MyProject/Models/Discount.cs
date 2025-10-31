using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MyProject.Models
{
    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = null!;

        [Required]
        public DateOnly ExpiryDate { get; set; }

        [Column(TypeName = "decimal(5, 2)")] // 999.99
        [Range(0.01, 100.00, ErrorMessage = "เรทส่วนลดต้องอยู่ระหว่าง 0.01 ถึง 100")]
        public decimal Rate { get; set; }

        // Navigation Properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}