using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Giveaway
{
    public int GId { get; set; }

    public string GName { get; set; } = null!;

    public int GPointcost { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
