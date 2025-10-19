using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
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
            bool skipViewCount = TempData["SkipViewCount"] as bool? ?? false;

            if (User.Identity != null && User.Identity.IsAuthenticated) // <--- เพิ่มเช็ค Login
            {
                try
                {
                    // *** (เพิ่ม) ดึง UserId ของคนที่ Login อยู่ ***
                    var userEmail = User.FindFirstValue(ClaimTypes.Email); // หรือ ClaimTypes.NameIdentifier
                    var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);

                    if (currentUser != null) // <--- เช็คว่าหา User เจอ
                    {
                        int userId = currentUser.UserId; // <--- ได้ UserId แล้ว

                        string currentMonthYear = DateTime.Now.ToString("yyyy-MM");
                        var favoriteRecord = await _context.Favorites
                            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == id && f.MonthYear == currentMonthYear); // <--- ค้นหาโดย UserId ด้วย

                        if (favoriteRecord != null)
                        {
                            favoriteRecord.ViewCount += 1;
                        }
                        else
                        {
                            _context.Favorites.Add(new Favorite
                            {
                                UserId = userId, // <--- กำหนด UserId ตอนสร้างใหม่
                                ProductId = id,
                                MonthYear = currentMonthYear,
                                ViewCount = 1
                            });
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex) { _logger.LogError(ex, "Error updating favorite view count for ProductId {ProductId}", id); }
            }
            else if (skipViewCount)
            {
                _logger.LogInformation("Skipping favorite view count update due to redirect for ProductId {ProductId}", id);
                TempData.Remove("SkipViewCount"); // ใช้แล้วลบออก
            }
            else
            {
                _logger.LogInformation("User not authenticated. Skipping favorite view count update for ProductId {ProductId}", id);
            }

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
                PricePerDay = product.PricePerDay,
                PricePerWeek = product.PricePerWeek,
                PricePerMonth = product.PricePerMonth,
                ImageUrl = product.ImageUrl,
                ImageUrl2 = product.ImageUrl2,

                // --- Map Specifications (ใช้ชื่อ Property จาก Model Product.cs) ---
                GearType = product.GearType,
                Engine = product.Engine,
                CoolingSystem = product.CoolingSystem,
                StartingSystem = product.StartingSystem, // Starting
                FuelType = product.FuelType,
                FuelDispensing = product.FuelDispensing, // Dispensing
                FuelTankCapacity = product.FuelTankCapacity, // TankCapacity
                BrakeSystem = product.BrakeSystem,
                Suspension = product.Suspension,
                TireSize = product.TireSize, // TireSize
                Dimensions = product.Dimensions,
                VehicleWeight = product.VehicleWeight
            };

            return View(viewModel);
        }
    }
}