using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MyProject.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // เช่น "Admin", "Customer"

        // Navigation Properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}