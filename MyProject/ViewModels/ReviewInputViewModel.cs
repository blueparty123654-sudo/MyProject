using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    public class ReviewInputViewModel
    {
        public int? ProductId { get; set; }
        public int? BranchId { get; set; }

        [Required(ErrorMessage = "กรุณาให้คะแนน")]
        [Range(1, 5, ErrorMessage = "คะแนนต้องอยู่ระหว่าง 1 ถึง 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "กรุณาแสดงความคิดเห็น")]
        [StringLength(1000, ErrorMessage = "ความคิดเห็นต้องไม่เกิน 1000 ตัวอักษร")]
        public string Comment { get; set; } = null!;
    }
}