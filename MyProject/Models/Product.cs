using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Product
{
    public int PrId { get; set; }

    public string PrName { get; set; } = null!;

    public decimal PrCostdaily { get; set; }

    public decimal PrCostweekly { get; set; }

    public decimal PrCostmonthly { get; set; }

    public string PrImage { get; set; } = null!;

    public string PrLocation { get; set; } = null!;

    public string PrGeartype { get; set; } = null!;

    public string PrEngine { get; set; } = null!;

    public string PrCoolin { get; set; } = null!;

    public string PrStartingsystem { get; set; } = null!;

    public string PrFule { get; set; } = null!;

    public string PrFuledispensing { get; set; } = null!;

    public string PrFuletank { get; set; } = null!;

    public string PrBrake { get; set; } = null!;

    public string PrSuspension { get; set; } = null!;

    public string PrTriesize { get; set; } = null!;

    public string PrSize { get; set; } = null!;

    public string PrVehicleweight { get; set; } = null!;

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
