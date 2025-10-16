using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public partial class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; } = null!; // เดิมคือ UserName

        [Required]
        public string Email { get; set; } = null!; // เดิมคือ UserEmail

        [Required]
        public string PhoneNumber { get; set; } = null!; // เดิมคือ UserNo

        [Required]
        public DateOnly DateOfBirth { get; set; } // เดิมคือ UserDob

        [Required]
        public string PasswordHash { get; set; } = null!; // เดิมคือ UserPass (ชื่อใหม่ชัดเจนกว่า)

        public string? DrivingLicenseImageUrl { get; set; } // เดิมคือ UserDrivingcard

        public int UserPoint { get; set; } = 0;

        public string? Status { get; set; }

        public int RoleId { get; set; } // Foreign Key

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        // --- Navigation Properties (ความสัมพันธ์กับตารางอื่น) ---
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}