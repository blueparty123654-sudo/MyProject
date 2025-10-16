using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Redemption
    {
        // Composite Primary Key
        public int UserId { get; set; }
        public int GiveawayId { get; set; }

        public DateTime RedemptionDate { get; set; }

        [Required]
        public string Status { get; set; } = "Processing"; // e.g., "Processing", "Shipped", "Completed"

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Giveaway Giveaway { get; set; } = null!;
    }
}