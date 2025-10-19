using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MyProject.Controllers
{
    [Authorize] // บังคับ Login
    public class BookingController : Controller
    {
        private readonly MyBookstoreDbContext _context;
        private readonly ILogger<BookingController> _logger;

        public BookingController(MyBookstoreDbContext context, ILogger<BookingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingInputViewModel model)
        {
            _logger.LogInformation("Booking Create POST received for ProductId: {ProductId}", model.ProductId);

            // --- 1. Basic & Date Validation ---
            if (!ModelState.IsValid || model.ReturnDate <= model.PickupDate)
            {
                TempData["BookingError"] = !ModelState.IsValid ? "ข้อมูลไม่ครบถ้วน" : "วันที่คืนรถต้องอยู่หลังวันที่รับรถ";
                _logger.LogWarning("Booking validation failed for ProductId: {ProductId}", model.ProductId);
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }

            if (model.PickupDate.Date < DateTime.Today)
            {
                TempData["BookingError"] = "วันที่รับรถต้องไม่ใช่วันในอดีต";
                _logger.LogWarning("Booking failed: PickupDate is in the past for ProductId: {ProductId}", model.ProductId);
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }

            // --- 2. Get User ---
            var userEmail = User.FindFirstValue(ClaimTypes.Email); // หรือ ClaimTypes.NameIdentifier ถ้าเก็บ ID
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail); // *** ตรวจสอบ Property Email/UserId ***
            if (currentUser == null)
            {
                TempData["BookingError"] = "ไม่พบข้อมูลผู้ใช้";
                _logger.LogError("Booking failed: User not found for email: {Email}", userEmail);
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }
            int userId = currentUser.UserId; // *** ตรวจสอบ Property UserId ***
            _logger.LogInformation("Booking user found: UserId: {UserId}", userId);

            // --- 3. Get Product ---
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == model.ProductId);
            if (product == null)
            {
                TempData["BookingError"] = "ไม่พบข้อมูลรถ";
                _logger.LogError("Booking failed: Product not found: {ProductId}", model.ProductId);
                return RedirectToAction("Index", "Home");
            }

            // --- 4. Determine Branch & Check Stock ---
            // สมมติว่าใช้สาขาแรกเป็น Default เสมอ
            var defaultBranch = await _context.Branches.OrderBy(b => b.BranchId).AsNoTracking().FirstOrDefaultAsync(); // *** ตรวจสอบ Property BranchId ***
            if (defaultBranch == null)
            {
                TempData["BookingError"] = "ไม่พบข้อมูลสาขา";
                _logger.LogError("Booking failed: Default branch not found.");
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }
            int branchIdToCheck = defaultBranch.BranchId; // *** ตรวจสอบ Property BranchId ***
            _logger.LogInformation("Booking for BranchId: {BranchId}", branchIdToCheck);

            // *** สำคัญ: แก้ไข Query ให้ดึง Stock อย่างถูกต้อง ***
            var branchProduct = await _context.BranchProducts
                .FirstOrDefaultAsync(bp => bp.BranchId == branchIdToCheck && bp.ProductId == model.ProductId); // *** ตรวจสอบ Property BranchId, ProductId ***

            if (branchProduct == null || branchProduct.StockQuantity <= 0) // *** ตรวจสอบ Property StockQuantity ***
            {
                TempData["BookingError"] = $"ขออภัย รถ {product.Name} หมดในสาขา {defaultBranch.Name} ชั่วคราว";
                _logger.LogWarning("Booking failed: Out of stock for ProductId {ProductId} at BranchId {BranchId}", model.ProductId, branchIdToCheck);
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }
            // TODO: (Advance) Check for existing bookings overlapping the selected dates

            // --- 5. Validate Discount & Calculate Price ---
            Discount? appliedDiscount = null; // เก็บ Discount object ที่ใช้ได้
            if (!string.IsNullOrWhiteSpace(model.DiscountCode))
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                appliedDiscount = await _context.Discounts.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Code == model.DiscountCode && d.Date >= today);
                if (appliedDiscount == null)
                {
                    // โค้ดผิด/หมดอายุ -> **ป้องกันการจอง**
                    TempData["BookingError"] = $"รหัสส่วนลด '{model.DiscountCode}' ไม่ถูกต้องหรือหมดอายุ (กรุณาลบหรือแก้ไข)";
                    _logger.LogWarning("Booking failed: Invalid discount code {Code} submitted.", model.DiscountCode);
                    return RedirectToAction("Details", "Product", new { id = model.ProductId });
                }
                _logger.LogInformation("Valid discount code {Code} confirmed server-side.", appliedDiscount.Code);
            }

            // --- 6. Calculate Original Price (Optimized) & Points ---
            TimeSpan duration = model.ReturnDate - model.PickupDate;
            int numberOfDays = duration.Days + 1; // นับวันแรกด้วย

            // --- (เพิ่ม) Logic คำนวณราคาแบบ Optimize (เดือน/สัปดาห์/วัน) สำหรับ basePrice ---
            decimal basePrice = 0;
            int remainingDays = numberOfDays;
            int months = 0, weeks = 0, days = 0;
            const int daysInMonth = 30; // สมมติฐาน
            const int daysInWeek = 7;

            if (product.PricePerMonth > 0 && remainingDays >= daysInMonth)
            {
                months = (int)Math.Floor((decimal)remainingDays / daysInMonth);
                basePrice += months * product.PricePerMonth;
                remainingDays %= daysInMonth;
            }
            if (product.PricePerWeek > 0 && remainingDays >= daysInWeek)
            {
                // เช็คความคุ้มค่า
                if (product.PricePerWeek < product.PricePerDay * daysInWeek)
                {
                    weeks = (int)Math.Floor((decimal)remainingDays / daysInWeek);
                    basePrice += weeks * product.PricePerWeek;
                    remainingDays %= daysInWeek;
                }
            }
            if (remainingDays > 0)
            {
                days = remainingDays;
                basePrice += days * product.PricePerDay;
            }
            // --- จบ Logic คำนวณราคา Optimize ---

            // --- (เพิ่ม) คำนวณ Point (ปัดขึ้น) ---
            int calculatedPoints = (int)Math.Ceiling(basePrice / 100);

            _logger.LogInformation("Calculated Base Price: {BasePrice}, Points: {Points}, Days: {Days}", basePrice, calculatedPoints, numberOfDays);

            // --- 7. Create Order ---
            var order = new Order
            {
                UserId = userId,
                ProductId = model.ProductId,
                DateReceipt = DateOnly.FromDateTime(model.PickupDate),
                DateReturn = DateOnly.FromDateTime(model.ReturnDate),
                Price = basePrice,              // *** ใช้ราคาก่อนหักส่วนลด ***
                Point = calculatedPoints,       // *** ใส่ Point ที่คำนวณแล้ว ***
                DiscountId = appliedDiscount?.DiscountId // เก็บ ID ส่วนลด (ถ้ามี)
                                                         // ตรวจสอบ Property อื่นๆ ที่จำเป็นใน Model Order ของคุณ
            };
            _context.Orders.Add(order);


            // --- 8. Update Stock ---
            // ดึง branchProduct มาอีกครั้งเพื่อ track การเปลี่ยนแปลง (ถ้าไม่ได้ดึงแบบ tracking ตั้งแต่แรก)
            var trackedBranchProduct = await _context.BranchProducts
                .FirstOrDefaultAsync(bp => bp.BranchId == branchIdToCheck && bp.ProductId == model.ProductId);
            if (trackedBranchProduct != null)
            {
                trackedBranchProduct.StockQuantity -= 1; // *** ตรวจสอบ Property StockQuantity ***
                _logger.LogInformation("Decremented stock for Product {ProdId} at Branch {BranchId}. New Stock: {Stock}", model.ProductId, branchIdToCheck, trackedBranchProduct.StockQuantity);
            }
            else
            {
                _logger.LogError("Failed to find BranchProduct record to decrement stock for Product {ProdId} at Branch {BranchId}", model.ProductId, branchIdToCheck);
                // อาจจะ Rollback transaction หรือแจ้ง Error ร้ายแรง
                TempData["BookingError"] = "เกิดข้อผิดพลาดร้ายแรงในการอัปเดตสต็อก";
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }


            // --- 9. Save Changes ---
            try
            {
                await _context.SaveChangesAsync(); // Save all changes together
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving booking changes for ProductId {ProductId}", model.ProductId);
                TempData["BookingError"] = "เกิดข้อผิดพลาดในการบันทึกข้อมูลการจอง";
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }

            // --- 10. Redirect ---
            if (branchProduct == null || branchProduct.StockQuantity <= 0)
            {
                TempData["BookingError"] = $"ขออภัย รถ {product.Name} หมดในสาขา {defaultBranch.Name} ชั่วคราว";
                TempData["SkipViewCount"] = true; // <--- (เพิ่ม) ส่ง Flag บอกว่าไม่ต้องนับ View
                _logger.LogWarning("Booking failed: Out of stock...");
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }
            TempData["BookingSuccess"] = $"การจองรถ {product.Name} สำเร็จ!"; // อาจจะไม่ต้องแสดง Order ID ถ้าไม่จำเป็น
            _logger.LogInformation("Booking successful for ProductId: {ProductId}, UserId: {UserId}", model.ProductId, userId);
            return RedirectToAction("Index", "Home");
        }
    }
}