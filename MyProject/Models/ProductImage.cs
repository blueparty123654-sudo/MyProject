using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class ProductImage
    {
        [Key]
        public int ProductImageId { get; set; }

        [Required]
        [StringLength(500)]
        public string Url { get; set; } = null!;

        [Range(1, 100, ErrorMessage = "must in between 1 - 100")]
        public int ImageNo { get; set; } = 1;

        // Foreign Key
        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}