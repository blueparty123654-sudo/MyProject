using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models; // *** ตรวจสอบว่า Models มีครบ: Order, OrderDetail, Discount, BranchProduct, User ***
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
            if (!ModelState.IsValid)
            {
                TempData["BookingError"] = "ข้อมูลไม่ครบถ้วน กรุณาเลือกวันที่รับและคืนรถ";
                _logger.LogWarning("Booking failed: ModelState invalid for ProductId: {ProductId}", model.ProductId);
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }
            if (model.ReturnDate <= model.PickupDate)
            {
                TempData["BookingError"] = "วันที่คืนรถต้องอยู่หลังวันที่รับรถ";
                _logger.LogWarning("Booking failed: ReturnDate <= PickupDate for ProductId: {ProductId}", model.ProductId);
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
            decimal discountAmount = 0; decimal discountPercentage = 0; Discount? appliedDiscount = null;
            if (!string.IsNullOrWhiteSpace(model.DiscountCode))
            {
                appliedDiscount = await _context.Discounts.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Code == model.DiscountCode && d.ExpiryDate >= DateTime.Today && d.IsActive); // *** ตรวจสอบ Property Code, ExpiryDate, IsActive ***
                if (appliedDiscount != null)
                {
                    discountPercentage = appliedDiscount.Percentage ?? 0; // *** ตรวจสอบ Property Percentage ***
                    discountAmount = appliedDiscount.Amount ?? 0;       // *** ตรวจสอบ Property Amount ***
                    _logger.LogInformation("Applied discount code: {Code}, Percentage: {Perc}, Amount: {Amt}", appliedDiscount.Code, discountPercentage, discountAmount);
                }
                else
                {
                    TempData["BookingWarning"] = $"รหัสส่วนลด '{model.DiscountCode}' ไม่ถูกต้องหรือหมดอายุ";
                    _logger.LogWarning("Invalid discount code attempted: {Code}", model.DiscountCode);
                }
            }
            TimeSpan duration = model.ReturnDate - model.PickupDate; int numberOfDays = duration.Days + 1;
            decimal basePrice = numberOfDays * product.PricePerDay; // *** ตรวจสอบ Property PricePerDay ***
            decimal finalPrice = basePrice;
            if (discountPercentage > 0) { finalPrice = basePrice * (1 - (discountPercentage / 100)); }
            if (discountAmount > 0) { finalPrice = Math.Max(0, finalPrice - discountAmount); }
            _logger.LogInformation("Calculated Price - Base: {Base}, Final: {Final}, Days: {Days}", basePrice, finalPrice, numberOfDays);


            // --- 6. Create Order ---
            var order = new Order
            {
                UserId = userId,                  // *** ตรวจสอบ Property UserId ***
                OrderDate = DateTime.Now,         // *** ตรวจสอบ Property OrderDate ***
                PickupDate = model.PickupDate,    // *** ตรวจสอบ Property PickupDate ***
                ReturnDate = model.ReturnDate,    // *** ตรวจสอบ Property ReturnDate ***
                TotalPrice = finalPrice,          // *** ตรวจสอบ Property TotalPrice ***
                Status = "Pending",               // *** ตรวจสอบ Property Status ***
                BranchId = branchIdToCheck,       // *** ตรวจสอบ Property BranchId ***
                DiscountId = appliedDiscount?.DiscountId // *** ตรวจสอบ Property DiscountId ***
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save Order first to get OrderId

            _logger.LogInformation("Created Order ID: {OrderId}", order.OrderId); // *** ตรวจสอบ Property OrderId ***

            // --- 7. Create OrderDetail ---
            var orderDetail = new OrderDetail
            {
                OrderId = order.OrderId,         // *** ตรวจสอบ Property OrderId ***
                ProductId = model.ProductId,    // *** ตรวจสอบ Property ProductId ***
                Quantity = 1,                 // *** ตรวจสอบ Property Quantity ***
                Price = product.PricePerDay     // *** ตรวจสอบ Property Price ***
                                                // หรือ Price = finalPrice ถ้าเก็บราคารวมรายการ
            };
            // *** ตรวจสอบ DbSet OrderDetails ใน DbContext ***
            _context.OrderDetails.Add(orderDetail);


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
            await _context.SaveChangesAsync(); // Save OrderDetail & Stock update

            // --- 10. Redirect ---
            TempData["BookingSuccess"] = $"การจองรถ {product.Name} สำเร็จ! (Order ID: {order.OrderId})";
            _logger.LogInformation("Booking successful for Order ID: {OrderId}", order.OrderId);
            return RedirectToAction("Index", "Home"); // Redirect ไปหน้าหลัก
        }
    }
}