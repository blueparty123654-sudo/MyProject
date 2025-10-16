namespace MyProject.Models
{
    public class Favorite
    {
        // ไม่มี [Key] ที่นี่ เพราะเราจะกำหนดใน DbContext
        public int UserId { get; set; }
        public int ProductId { get; set; } // เดิมคือ PrId
        public string? MonthYear { get; set; } // เดิมคือ FavMonthYear
        public int ViewCount { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual Product? Product { get; set; }
    }
}