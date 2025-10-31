using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Giveaway
    {
        [Key]
        public int GiveawayId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "PointCost must not be negative")]
        public int PointCost { get; set; }

        // Navigation Properties
        public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
    }
}