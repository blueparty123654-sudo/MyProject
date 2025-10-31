using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class Redemption
    {
        [Key]
        public int RedemptionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int GiveawayId { get; set; }

        [Required]
        public DateTime RedemptionDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Processing";

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        [ForeignKey("GiveawayId")]
        public virtual Giveaway Giveaway { get; set; } = null!;
    }
}