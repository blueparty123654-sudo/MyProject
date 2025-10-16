using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Discount
    {
        [Key] // 👈 ติดป้ายบอกว่านี่คือ Primary Key
        public int DiscountId { get; set; } // เดิมคือ DcId

        public string Code { get; set; } = null!; // เดิมคือ DcCode
        public DateOnly Date { get; set; } // เดิมคือ DcDate
        public decimal Rate { get; set; } // เดิมคือ DcRates
    }
}