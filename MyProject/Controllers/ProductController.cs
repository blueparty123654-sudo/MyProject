using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly MyBookstoreDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(MyBookstoreDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Product/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            // --- 1. Increment View Count ---
            try
            {
                string currentMonthYear = DateTime.Now.ToString("yyyy-MM");
                var favoriteRecord = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.ProductId == id && f.MonthYear == currentMonthYear);
                if (favoriteRecord != null) { favoriteRecord.ViewCount += 1; }
                else { _context.Favorites.Add(new Favorite { ProductId = id, MonthYear = currentMonthYear, ViewCount = 1 }); }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error updating favorite view count for ProductId {ProductId}", id); }

            // --- 2. Fetch Product Details ---
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) { return NotFound(); }

            // --- 3. Create ViewModel ---
            var viewModel = new ProductDetailsViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description, // *** ตรวจสอบ ***
                PricePerDay = product.PricePerDay,
                PricePerWeek = product.PricePerWeek,
                PricePerMonth = product.PricePerMonth,
                ImageUrl = product.ImageUrl,
                ImageUrl2 = product.ImageUrl2,

                // --- Map Specifications (ใช้ชื่อ Property จาก Model Product.cs) ---
                // *** ตรวจสอบชื่อ Property ของ Entity 'product' ให้ตรงกับ Product.cs ที่แก้แล้ว ***
                ModelYear = product.ModelYear,
                Gear = product.Gear,
                Engine = product.Engine,
                Cooling = product.Cooling,
                Starter = product.Starter,
                CC = product.CC,
                Fuel = product.Fuel,
                FuelSystem = product.FuelSystem,
                FuelCapacity = product.FuelCapacity,
                Suspension = product.Suspension,
                Brake = product.Brake,
                Wheels = product.Wheels,
                Tires = product.Tires,
                Dimensions = product.Dimensions,
                Weight = product.Weight
                // เพิ่ม Property อื่นๆ ให้ครบ...
            };

            return View(viewModel);
        }
    }
}