using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Order
{
    public int OdId { get; set; }

    public decimal OdPrice { get; set; }

    public int OdPoint { get; set; }

    public DateOnly OdDateReceipt { get; set; }

    public DateOnly OdDateReturn { get; set; }

    public string RentalType { get; set; } = null!;

    public int PayId { get; set; }

    public int PrId { get; set; }

    public int UserId { get; set; }

    public virtual Payment Pay { get; set; } = null!;

    public virtual Product Pr { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Discount> Dcs { get; set; } = new List<Discount>();
}
