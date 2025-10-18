using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
                .ToListAsync();

            int actualBranchIdToShow = branchId ?? filterBranches.FirstOrDefault()?.BranchId ?? 0;

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
                    .FirstOrDefaultAsync();
            }

            ViewBag.BranchDetailsForLayout = selectedBranchForFooter;

            string? selectedNameForDisplay = null;

            if (branchId.HasValue)
            {
                // ถ้ามีการคลิก ให้หาชื่อสาขาที่ตรงกับ branchId ที่ส่งมา
                selectedNameForDisplay = filterBranches.FirstOrDefault(b => b.BranchId == branchId.Value)?.Name;
                // ถ้าหาไม่เจอ (อาจเกิดได้ยาก) ให้ใช้ค่าสำรอง
                if (string.IsNullOrEmpty(selectedNameForDisplay))
                {
                    selectedNameForDisplay = "สาขาที่เลือก";
                }
            }

            if (actualBranchIdToShow == 0 && !filterBranches.Any())
            {
                selectedNameForDisplay = "ไม่มีสาขา"; // แสดงข้อความพิเศษนี้
            }

            var productQuery = _context.Products.AsQueryable();
            if (actualBranchIdToShow > 0) // กรองตามสาขาเสมอ (ถ้ามีสาขา)
            {
                productQuery = productQuery
                    .Where(p => _context.BranchProducts
                                    .Any(bp => bp.BranchId == actualBranchIdToShow && bp.ProductId == p.ProductId));
            }
   

            // 4. ดึงข้อมูล Product (ที่กรองแล้ว)
            var products = await productQuery
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    PricePerDay = p.PricePerDay,
                    PricePerWeek = p.PricePerWeek,
                    PricePerMonth = p.PricePerMonth,
                    ImageUrl = p.ImageUrl,
                    IsAvailable = (actualBranchIdToShow > 0)
                          ? _context.BranchProducts
                          .Any(bp => bp.BranchId == actualBranchIdToShow && bp.ProductId == p.ProductId && bp.StockQuantity > 0) : false
                })
                .ToListAsync();

            var viewModel = new HomePageViewModel
            {
                Products = products,
                FilterBranches = filterBranches,
                SelectedBranchId = actualBranchIdToShow > 0 ? actualBranchIdToShow : (int?)null,
                SelectedFilterName = selectedNameForDisplay,
                SelectedBranchDetails = selectedBranchForFooter // ข้อมูลสำหรับ Footer
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