using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; } // เดิมคือ BrId

        [Required]
        public string Name { get; set; } = string.Empty; // เดิมคือ BrName

        [Required]
        public string Address { get; set; } = string.Empty; // เดิมคือ BrAddress

        [Required]
        public string PhoneNumber { get; set; } = string.Empty; // เดิมคือ BrPhoneNumber

        public string? MapUrl { get; set; } // เดิมคือ BrMapUrl

        // --- Navigation Property ---
        // ความสัมพันธ์: 1 สาขา มีได้หลาย Product
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}