using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class ProductDetail
    {
        [Key]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [StringLength(100)]
        public string GearType { get; set; } = null!;
        [StringLength(100)]
        public string Engine { get; set; } = null!;
        [StringLength(100)]
        public string CoolingSystem { get; set; } = null!;
        [StringLength(100)]
        public string StartingSystem { get; set; } = null!;
        [StringLength(100)]
        public string FuelType { get; set; } = null!;
        [StringLength(100)]
        public string FuelDispensing { get; set; } = null!;
        [StringLength(50)]
        public string FuelTankCapacity { get; set; } = null!;
        [StringLength(100)]
        public string BrakeSystem { get; set; } = null!;
        [StringLength(100)]
        public string Suspension { get; set; } = null!;
        [StringLength(100)]
        public string TireSize { get; set; } = null!;
        [StringLength(100)]
        public string Dimensions { get; set; } = null!;
        [StringLength(50)]
        public string VehicleWeight { get; set; } = null!;

        // Navigation Property กลับไปหา Product
        public virtual Product Product { get; set; } = null!;
    }
}