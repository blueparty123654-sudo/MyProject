using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Payment
{
    public int PayId { get; set; }

    public string PayMethod { get; set; } = null!;

    public decimal PayAmount { get; set; }

    public DateTime PayDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
