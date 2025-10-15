using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string UserNo { get; set; } = null!;

    public DateOnly UserDob { get; set; }

    public string UserPass { get; set; } = null!;

    public string UserRole { get; set; } = null!;

    public int UserPoint { get; set; }

    public string? UserDrivingcard { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Discount> Dcs { get; set; } = new List<Discount>();

    public virtual ICollection<Giveaway> GIds { get; set; } = new List<Giveaway>();
}
