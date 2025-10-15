using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Review
{
    public int RvId { get; set; }

    public string? RvText { get; set; }

    public int? RvRating { get; set; }

    public int? PrId { get; set; }

    public int? UserId { get; set; }

    public virtual Product? Pr { get; set; }

    public virtual User? User { get; set; }
}
