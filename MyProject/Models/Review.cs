using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int? ProductId { get; set; }
        public int? BranchId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = null!;

        public DateTime ReviewDate { get; set; }

        // Navigation Properties

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }
    }
}