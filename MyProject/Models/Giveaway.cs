using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Giveaway
    {
        [Key]
        public int GiveawayId { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string ImageUrl { get; set; } = string.Empty; // 👈 **เพิ่มบรรทัดนี้เข้ามา**
        public int PointCost { get; set; }

        // ความสัมพันธ์กับ Redemption
        public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
    }
}