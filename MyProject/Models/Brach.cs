using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? MapUrl { get; set; }

        // --- Navigation Property ---
        public virtual ICollection<BranchProduct> BranchProducts { get; set; } = new List<BranchProduct>();
    }
}