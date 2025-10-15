using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Discount
{
    public int DcId { get; set; }

    public DateOnly DcDate { get; set; }

    public decimal DcRates { get; set; }

    public string DcCode { get; set; } = null!;

    public virtual ICollection<Order> Ods { get; set; } = new List<Order>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
