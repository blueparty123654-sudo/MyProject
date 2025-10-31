using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Favorite
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }

        [StringLength(7)]
        public string? MonthYear { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "ViewCount must not be negative")]
        public int ViewCount { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual Product? Product { get; set; }
    }
}