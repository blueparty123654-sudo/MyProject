using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyBookstoreDbContext _context; 

        public HomeController(ILogger<HomeController> logger, MyBookstoreDbContext context) 
        {
            _logger = logger;
            _context = context;
        }

        // --- Action Index ---
        public async Task<IActionResult> Index(int? branchId = null)
        {

            var filterBranches = await _context.Branches
                .OrderBy(b => b.Name)
                .Select(b => new BranchFilterViewModel
                {
                    BranchId = b.BranchId,
                    Name = b.Name
                })
                .AsNoTracking()
                .ToListAsync();

            int actualBranchIdToShow;
            if (branchId.HasValue)
            {
                // ถ้ามีการส่ง ID มาทาง URL (คลิก Filter) ให้ใช้ ID นั้น
                actualBranchIdToShow = branchId.Value;
            }
            else
            {
                // ถ้าไม่ได้ส่ง ID มา (โหลดครั้งแรก) ให้พยายามใช้ BranchId = 1 เป็น Default
                actualBranchIdToShow = filterBranches.Any(b => b.BranchId == 1)
                                        ? 1 // ถ้ามี ให้ใช้ 1
                                        : filterBranches.FirstOrDefault()?.BranchId ?? 0;
            }

            SelectedBranchDetailsViewModel? selectedBranchForFooter = null;
            if (actualBranchIdToShow > 0)
            {
                selectedBranchForFooter = await _context.Branches
                    .Where(b => b.BranchId == actualBranchIdToShow)
                    .Select(b => new SelectedBranchDetailsViewModel
                    {
                        BranchId = b.BranchId,
                        Name = b.Name,
                        Address = b.Address,
                        PhoneNumber = b.PhoneNumber,
                        MapUrl = b.MapUrl
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            // ----- จบส่วนที่เพิ่ม -----

            // ----- (เพิ่ม) กำหนดค่า ViewBag สำหรับ Layout -----
            ViewBag.BranchDetailsForLayout = selectedBranchForFooter;

            string? selectedNameForDisplay = null;
            if (selectedBranchForFooter != null) // ใช้ selectedBranchForFooter
            {
                selectedNameForDisplay = selectedBranchForFooter.Name;
            }
            else if (branchId.HasValue) { selectedNameForDisplay = "สาขาที่เลือก (ไม่พบ)"; }
            else if (actualBranchIdToShow == 0 && !filterBranches.Any()) { selectedNameForDisplay = "ไม่มีสาขา"; }

            // 4. ดึงข้อมูล Product (ที่กรองแล้ว)
            // ----- (แก้ไข) กำหนดค่าให้ products ที่ประกาศไว้ข้างนอก -----
            List<ProductViewModel> products = new List<ProductViewModel>();

            products = await _context.Products // <--- ไม่ต้องใช้ productQuery แล้ว เริ่มจาก _context.Products ได้เลย
                .Join(
                    _context.BranchProducts.Where(bp => bp.BranchId == actualBranchIdToShow),
                    product => product.ProductId,
                    branchProduct => branchProduct.ProductId,
                    (product, branchProduct) => new { product, branchProduct.StockQuantity } // ส่ง product ทั้ง object ไปเลยง่ายกว่า
                )
                .Select(joined => new ProductViewModel // แปลงเป็น ViewModel
                {
                    ProductId = joined.product.ProductId,
                    Name = joined.product.Name,
                    PricePerDay = joined.product.PricePerDay,
                    PricePerWeek = joined.product.PricePerWeek,
                    PricePerMonth = joined.product.PricePerMonth,
                    ImageUrl = joined.product.ImageUrl,
                    IsAvailable = joined.StockQuantity > 0
                })
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new HomePageViewModel
            {
                Products = products,
                FilterBranches = filterBranches,
                SelectedBranchId = actualBranchIdToShow > 0 ? actualBranchIdToShow : (int?)null,
                SelectedFilterName = selectedNameForDisplay,
            };

            // ส่ง ViewModel ไปที่ View
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }   
}