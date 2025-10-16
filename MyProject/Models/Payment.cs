using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class Payment
    {
        [Key] // 👈 ติดป้าย
        public int PaymentId { get; set; } // เดิมคือ PayId

        public decimal Amount { get; set; } // เดิมคือ PayAmount
        public DateTime Date { get; set; } // เดิมคือ PayDate
        public string Method { get; set; } = null!; // เดิมคือ PayMethod
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}