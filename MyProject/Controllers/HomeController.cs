using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System.Diagnostics;

namespace MyProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyBookstoreDbContext _context; // 👈 (เพิ่ม) DbContext

        public HomeController(ILogger<HomeController> logger, MyBookstoreDbContext context) // 👈 (เพิ่ม) DbContext
        {
            _logger = logger;
            _context = context; // 👈 (เพิ่ม) DbContext
        }

        // --- Action Index ---
        public async Task<IActionResult> Index()
        {
            // 1. ไป "หยิบ" ข้อมูลรถทั้งหมดจากฐานข้อมูล
            var products = await _context.Products
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    PricePerDay = p.PricePerDay,
                    PricePerWeek = p.PricePerWeek,
                    PricePerMonth = p.PricePerMonth,
                    ImageUrl = p.ImageUrl // ImageUrl จะมาจากฐานข้อมูลโดยตรง
                })
                .ToListAsync();

            // 2. "เสิร์ฟ" ข้อมูลรถไปที่ View ของหน้า Index
            return View(products);
        }

        // --- Action Reviews ---
        public async Task<IActionResult> Reviews()
        {
            var viewModel = new ReviewPageViewModel();

            // 1. ดึงข้อมูลสำหรับ Dropdown
            viewModel.Products = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.ProductId.ToString(), Text = p.Name })
                .ToListAsync();

            viewModel.Branches = await _context.Branches
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.BranchId.ToString(), Text = b.Name })
                .ToListAsync();

            // 2. ดึงรีวิวล่าสุด (เช่น 10 อันล่าสุด)
            var rawReviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.Branch)
                .OrderByDescending(r => r.ReviewDate)
                .Take(10)
            .Select(r => new // สร้าง Object ชั่วคราวขึ้นมา
            {
                UserName = r.User.Name,
                ProductName = r.Product != null ? r.Product.Name : null,
                BranchName = r.Branch != null ? r.Branch.Name : null,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewDate = r.ReviewDate // 👈 ดึงวันที่มาแบบดิบๆ
            })
            .ToListAsync();

            // 2. แปลงข้อมูลดิบให้เป็น ViewModel (ขั้นตอนนี้ทำงานใน C# ไม่เกี่ยวกับฐานข้อมูลแล้ว)
            viewModel.Reviews = rawReviews.Select(r => new ReviewItemViewModel
            {
                UserName = r.UserName ?? "Anonymous",
                ProductName = r.ProductName,
                BranchName = r.BranchName,
                Rating = r.Rating,
                Comment = r.Comment,
                PostedAgo = TimeAgo(r.ReviewDate) // 👈 เรียกใช้ TimeAgo ที่นี่ได้อย่างปลอดภัย
            }).ToList();

            return View(viewModel);
        }

        private string TimeAgo(DateTime dt)
        {
            TimeSpan span = DateTime.Now - dt;
            if (span.Days > 365) return $"{span.Days / 365} ปีที่แล้ว";
            if (span.Days > 30) return $"{span.Days / 30} เดือนที่แล้ว";
            if (span.Days > 0) return $"{span.Days} วันที่แล้ว";
            if (span.Hours > 0) return $"{span.Hours} ชั่วโมงที่แล้ว";
            if (span.Minutes > 0) return $"{span.Minutes} นาทีที่แล้ว";
            return "เมื่อสักครู่";
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}