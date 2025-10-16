using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Giveaway
    {
        [Key] // 👈 ติดป้าย
        public int GiveawayId { get; set; } // เดิมคือ GId

        public string Name { get; set; } = null!; // เดิมคือ GName
        public int PointCost { get; set; } // เดิมคือ GPointcost
    }
}