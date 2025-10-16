using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Review
    {
        [Key] // 👈 ติดป้าย
        public int ReviewId { get; set; } // เดิมคือ RvId

        public int UserId { get; set; }
        public int ProductId { get; set; } // เดิมคือ PrId
        public int Rating { get; set; } // เดิมคือ RvRating
        public string? Text { get; set; } // เดิมคือ RvText

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual Product? Product { get; set; }
    }
}