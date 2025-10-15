using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Favorite
{
    public int UserId { get; set; }

    public int PrId { get; set; }

    public string? FavMonthYear { get; set; }

    public int? ViewCount { get; set; }

    public virtual Product Pr { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
