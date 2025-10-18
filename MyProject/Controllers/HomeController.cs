using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
    }   
}