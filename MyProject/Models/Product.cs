using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public partial class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.0, double.MaxValue, ErrorMessage = "PricePerDay must not be negative")]
        public decimal PricePerDay { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.0, double.MaxValue, ErrorMessage = "PricePerWeek must not be negative")]
        public decimal PricePerWeek { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.0, double.MaxValue, ErrorMessage = "PricePerMonth must not be negative")]
        public decimal PricePerMonth { get; set; }

        // --- Navigation Properties ---
        public virtual ICollection<BranchProduct> BranchProducts { get; set; } = new List<BranchProduct>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ProductDetail? ProductDetail { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}