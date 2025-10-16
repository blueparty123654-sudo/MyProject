using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // เช่น "Admin", "Customer"

        // ความสัมพันธ์: 1 Role มีได้หลาย User
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}