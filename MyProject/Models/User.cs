using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public partial class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public string PasswordHash { get; set; } = null!;

        [StringLength(500)]
        public string? DrivingLicenseImageUrl { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "UserPoint must not be negative")]
        public int UserPoint { get; set; } = 0;

        [StringLength(50)]
        public string? Status { get; set; }

        [Required]
        public int RoleId { get; set; } // Foreign Key

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; } = null!;

        // --- Navigation Properties ---
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
    }
}