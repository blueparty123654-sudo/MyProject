using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public partial class Product
    {
        [Key]
        public int ProductId { get; set; } // เดิมคือ PrId

        [Required]
        public string Name { get; set; } = null!; // เดิมคือ PrName

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerDay { get; set; } // เดิมคือ PrCostdaily

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerWeek { get; set; } // เดิมคือ PrCostweekly

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerMonth { get; set; } // เดิมคือ PrCostmonthly

        [Required]
        public string ImageUrl { get; set; } = null!; // เดิมคือ PrImage

        public string GearType { get; set; } = null!; // เดิมคือ PrGeartype
        public string Engine { get; set; } = null!; // เดิมคือ PrEngine
        public string CoolingSystem { get; set; } = null!; // เดิมคือ PrCoolin
        public string StartingSystem { get; set; } = null!; // เดิมคือ PrStartingsystem
        public string FuelType { get; set; } = null!; // เดิมคือ PrFule
        public string FuelDispensing { get; set; } = null!; // เดิมคือ PrFuledispensing
        public string FuelTankCapacity { get; set; } = null!; // เดิมคือ PrFuletank
        public string BrakeSystem { get; set; } = null!; // เดิมคือ PrBrake
        public string Suspension { get; set; } = null!; // เดิมคือ PrSuspension
        public string TireSize { get; set; } = null!; // เดิมคือ PrTriesize
        public string Dimensions { get; set; } = null!; // เดิมคือ PrSize
        public string VehicleWeight { get; set; } = null!; // เดิมคือ PrVehicleweight

        // --- Navigation Properties ---
        public virtual ICollection<BranchProduct> BranchProducts { get; set; } = new List<BranchProduct>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}