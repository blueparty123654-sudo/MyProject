using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MyProject.Models
{
    public class Order : IValidatableObject
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int BranchId { get; set; }

        public int? DiscountId { get; set; }

        [Required]
        public DateOnly DateReceipt { get; set; }

        [Required]
        public DateOnly DateReturn { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Price ต้องไม่ติดลบ")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Point ต้องไม่ติดลบ")]
        public int Point { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Branch? Branch { get; set; }
        public virtual Payment? Payment { get; set; }
        public virtual Discount? Discount { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateReturn <= DateReceipt)
            {
                // ส่ง Error นี้กลับไปให้ ModelState
                yield return new ValidationResult(
                    "วันที่คืน (DateReturn) ต้องอยู่หลังจากวันที่รับ (DateReceipt)",
                    new[] { nameof(DateReturn), nameof(DateReceipt) }
                );
            }
        }
    }
}