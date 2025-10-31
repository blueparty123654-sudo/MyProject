using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data; // <<< ตรวจสอบ Namespace
using MyProject.Models; // <<< ตรวจสอบ Namespace
using MyProject.ViewModels; // <<< ตรวจสอบ Namespace
using System;
using System.Linq; // <<< เพิ่ม using System.Linq
using System.Security.Claims;
// using System.Text.RegularExpressions; // อาจจะไม่ต้องใช้ Regex แล้ว
using System.Threading.Tasks;

namespace MyProject.Controllers // <<< ตรวจสอบ Namespace
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly MyBookstoreDbContext _context; // <<< ตรวจสอบชื่อ DbContext
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(MyBookstoreDbContext context, ILogger<PaymentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- GET Index (เหมือนเดิม) ---
        [HttpGet]
        public async Task<IActionResult> Index(int orderId)
        {
            // ... (โค้ด GET Index เดิม) ...
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail)) return Challenge();

            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null) return Challenge();
            int currentUserId = currentUser.UserId;

            var order = await _context.Orders
                .Include(o => o.Product)
                    .ThenInclude(p => p!.ProductImages)
                .Include(o => o.Discount)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == currentUserId);

            if (order == null)
            {
                _logger.LogWarning("Payment GET failed: Order {OrderId} not found or access denied for UserId {UserId}", orderId, currentUserId);
                TempData["ErrorMessage"] = "ไม่พบรายการจองที่ต้องการชำระเงิน หรือคุณไม่มีสิทธิ์";
                return RedirectToAction("Index", "Home");
            }

            decimal originalPrice = order.Price;
            decimal discountAmount = 0;
            if (order.Discount != null && order.Discount.Rate > 0)
            {
                discountAmount = Math.Round(originalPrice * (order.Discount.Rate / 100m), 2);
            }
            decimal finalPrice = originalPrice - discountAmount;

            var viewModel = new PaymentViewModel
            {
                OrderId = order.OrderId,
                ProductName = order.Product?.Name,
                ProductImageUrl = order.Product?.ProductImages?.OrderBy(img => img.ImageNo)
                .FirstOrDefault()?.Url ?? "/images/placeholder.png",
                PickupDate = order.DateReceipt,
                ReturnDate = order.DateReturn,
                FinalPrice = finalPrice
            };

            return View(viewModel);
        }

        // --- POST: รับข้อมูลการชำระเงิน (แก้ไข Validation) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PaymentViewModel model)
        {
            // --- Validation แบบง่าย ---
            // 1. ต้องเลือก Method
            if (string.IsNullOrWhiteSpace(model.SelectedPaymentMethod))
            {
                ModelState.AddModelError(nameof(model.SelectedPaymentMethod), "กรุณาเลือกวิธีการชำระเงิน");
            }
            // 2. ถ้าเลือก Card ต้องกรอกข้อมูลให้ครบ
            else if (model.SelectedPaymentMethod == "Card")
            {
                if (string.IsNullOrWhiteSpace(model.CardHolderName))
                    ModelState.AddModelError(nameof(model.CardHolderName), "กรุณากรอกชื่อบนบัตร");
                if (string.IsNullOrWhiteSpace(model.CardNumber)) // ไม่เช็ค Pattern แล้ว
                    ModelState.AddModelError(nameof(model.CardNumber), "กรุณากรอกหมายเลขบัตร");
                if (string.IsNullOrWhiteSpace(model.ExpiryDate)) // ไม่เช็ค Pattern แล้ว
                    ModelState.AddModelError(nameof(model.ExpiryDate), "กรุณากรอกวันหมดอายุ");
                if (string.IsNullOrWhiteSpace(model.Cvv)) // ไม่เช็ค Pattern แล้ว
                    ModelState.AddModelError(nameof(model.Cvv), "กรุณากรอก CVV");
            }
            // --- จบ Validation ---

            // ถ้า Validation ไม่ผ่าน (ข้อมูลไม่ครบ) -> คืน JSON Error
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Payment POST failed: Model validation failed for OrderId {OrderId}", model.OrderId);
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                           .Select(e => e.ErrorMessage)
                                           .ToList();
                // ส่ง Error กลับไปให้ SweetAlert แสดง
                return Json(new { success = false, message = "ข้อมูลไม่ครบถ้วน กรุณาตรวจสอบ", errors = errors });
            }

            // --- ถ้า Validation ผ่าน ---
            // ดึง UserId (เหมือนเดิม)
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null)
            {
                // ควรคืน Json Error แทน Challenge สำหรับ AJAX
                return Json(new { success = false, message = "Session หมดอายุ กรุณา Login ใหม่" });
            }
            int currentUserId = currentUser.UserId;

            _logger.LogInformation("Processing payment for OrderId {OrderId}, Method: {PaymentMethod}, UserId {UserId}",
                model.OrderId, model.SelectedPaymentMethod, currentUserId);

            // ดึง Order (เหมือนเดิม)
            var order = await _context.Orders
                                    .Include(o => o.Discount)
                                    .FirstOrDefaultAsync(o => o.OrderId == model.OrderId && o.UserId == currentUserId);
            if (order == null)
            {
                _logger.LogWarning("Payment POST failed: Order {OrderId} not found or access denied for UserId {UserId}", model.OrderId, currentUserId);
                return Json(new { success = false, message = "ไม่พบรายการจองที่ต้องการชำระเงิน" });
            }


            // --- จำลองว่าสำเร็จเสมอ ---
            _logger.LogInformation("Simulating successful payment for OrderId {OrderId}", model.OrderId);

            try
            {
                // คำนวณ Final Price (เหมือนเดิม)
                decimal finalPriceToPay = order.Price - (order.Discount != null && order.Discount.Rate > 0 ? Math.Round(order.Price * (order.Discount.Rate / 100m), 2) : 0);

                // สร้าง Record Payment
                var paymentRecord = new Payment
                {
                    OrderId = order.OrderId,
                    Method = model.SelectedPaymentMethod ?? "Unknown",
                    Amount = finalPriceToPay,
                    Date = DateTime.UtcNow,
                    Status = "In progress"
                };
                _context.Payments.Add(paymentRecord);

                int pointsEarned = order.Point;
                if (pointsEarned > 0) // เพิ่ม Point เฉพาะเมื่อมี Point ให้เพิ่ม
                {
                    currentUser.UserPoint += pointsEarned; // <<<--- ตรวจสอบว่า User Model มี Property 'UserPoint' หรือไม่
                    _logger.LogInformation("Added {Points} points to UserId {UserId}. New total: {TotalPoints}",
                        pointsEarned, currentUserId, currentUser.UserPoint);
                }
                else
                {
                    _logger.LogInformation("No points earned for OrderId {OrderId}.", order.OrderId);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment record saved. User points updated for OrderId {OrderId}, PaymentId {PaymentId}.", order.OrderId, paymentRecord.PaymentId);

                // --- ส่ง JSON Success กลับไป ---
                int countdownSeconds = 3; // ลดเวลานับถอยหลังเหลือ 3 วิ
                return Json(new
                {
                    success = true,
                    message = $"ชำระเงินสำหรับ Order #{order.OrderId} สำเร็จ! กำลังกลับสู่หน้าหลักใน {countdownSeconds} วินาที...",
                    redirectUrl = Url.Action("Index", "Home"),
                    countdownSeconds = countdownSeconds
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving payment record for OrderId {OrderId}", model.OrderId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการบันทึกข้อมูล" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing payment for OrderId {OrderId}", model.OrderId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดไม่คาดคิด" });
            }
        }

    } // ปิด Class Controller
} // ปิด Namespace