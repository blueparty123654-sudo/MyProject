using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 👈 เพิ่ม using นี้

namespace MyProject.Models
{
    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        public string Code { get; set; } = null!;
        public DateOnly Date { get; set; }

        [Column(TypeName = "decimal(5, 2)")] // 👈 เพิ่มบรรทัดนี้ (ตัวอย่าง: 999.99)
        public decimal Rate { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}