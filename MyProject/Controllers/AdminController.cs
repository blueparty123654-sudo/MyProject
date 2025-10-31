using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.ViewModels;
using MyProject.Models;
using System.Linq;
using System.Collections.Generic;

namespace MyProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MyBookstoreDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(MyBookstoreDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ManagePayments(string filterStatus = "In progress") // Filter เริ่มต้นเป็น "In progress"
        {
            _logger.LogInformation("Admin accessing ManagePayments page with filter: {FilterStatus}", filterStatus);

            IQueryable<Payment> query = _context.Payments
                                            .Include(p => p.Order)
                                                .ThenInclude(o => o!.User)
                                            .Include(p => p.Order)
                                                .ThenInclude(o => o!.Product);

            // Filter ตาม Status (ถ้ามีการเลือก Filter)
            if (!string.IsNullOrEmpty(filterStatus) && filterStatus != "All")
            {
                query = query.Where(p => p.Status == filterStatus);
            }

            var payments = await query
                                .OrderByDescending(p => p.Date)
                                .Select(p => new AdminPaymentViewModel
                                {
                                    PaymentId = p.PaymentId,
                                    OrderId = p.OrderId,
                                    UserName = p.Order != null && p.Order.User != null ? p.Order.User.Name : "N/A",
                                    ProductName = p.Order != null && p.Order.Product != null ? p.Order.Product.Name : "N/A", // (Optional)
                                    PaymentDate = p.Date,
                                    Amount = p.Amount,
                                    Method = p.Method,
                                    CurrentStatus = p.Status,
                                    NewStatus = p.Status // ค่าเริ่มต้นของ Dropdown คือ Status ปัจจุบัน
                                })
                                .ToListAsync();

            ViewBag.FilterStatus = filterStatus; // ส่ง Filter ปัจจุบันไปให้ View
            ViewBag.AvailableStatuses = AdminPaymentViewModel.AvailableStatuses; // ส่ง List Status ไปให้ View

            return View(payments); // ส่ง List<AdminPaymentViewModel> ไป
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int paymentId, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus) || !AdminPaymentViewModel.AvailableStatuses.Contains(newStatus))
            {
                TempData["AdminError"] = "สถานะที่เลือกไม่ถูกต้อง";
                return RedirectToAction("ManagePayments");
            }

            var paymentToUpdate = await _context.Payments.FindAsync(paymentId);

            if (paymentToUpdate == null)
            {
                TempData["AdminError"] = $"ไม่พบ Payment ID: {paymentId}";
                return RedirectToAction("ManagePayments");
            }

            string oldStatus = paymentToUpdate.Status; // เก็บ Status เดิมไว้ Log
            paymentToUpdate.Status = newStatus;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin updated PaymentId {PaymentId} status from '{OldStatus}' to '{NewStatus}'", paymentId, oldStatus, newStatus);
                TempData["AdminSuccess"] = $"อัปเดตสถานะ Payment ID: {paymentId} เป็น '{newStatus}' เรียบร้อยแล้ว";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating status for PaymentId {PaymentId}", paymentId);
                TempData["AdminError"] = $"เกิดข้อผิดพลาดในการบันทึก Payment ID: {paymentId}";
            }

            // Redirect กลับไปหน้าเดิม พร้อม Filter เดิม (ถ้ามี)
            // เราอาจจะต้องส่ง Filter กลับมาด้วย หรือให้ User เลือก Filter ใหม่
            return RedirectToAction("ManagePayments");
        }

        // --- จัดการ Redemptions ---
        [HttpGet]
        public async Task<IActionResult> ManageRedemptions(string filterStatus = "Processing") // Filter เริ่มต้นเป็น "Processing"
        {
            _logger.LogInformation("Admin accessing ManageRedemptions page with filter: {FilterStatus}", filterStatus);

            IQueryable<Redemption> query = _context.Redemptions
                                                .Include(r => r.User)
                                                .Include(r => r.Giveaway);

            // Filter ตาม Status (ถ้ามีการเลือก Filter)
            if (!string.IsNullOrEmpty(filterStatus) && filterStatus != "All")
            {
                query = query.Where(r => r.Status == filterStatus);
            }

            var redemptions = await query
                                .OrderByDescending(r => r.RedemptionDate)   
                                .Select(r => new AdminRedemptionViewModel
                                {
                                    RedemptionId = r.RedemptionId,
                                    UserId = r.UserId,
                                    UserName = r.User != null ? r.User.Name : "N/A",
                                    GiveawayId = r.GiveawayId,
                                    GiveawayName = r.Giveaway != null ? r.Giveaway.Name : "N/A",
                                    RedemptionDate = r.RedemptionDate,
                                    CurrentStatus = r.Status,
                                    NewStatus = r.Status // ค่าเริ่มต้น Dropdown
                                })
                                .ToListAsync();

            ViewBag.FilterStatus = filterStatus; // ส่ง Filter ปัจจุบันไปให้ View
            ViewBag.AvailableStatuses = AdminRedemptionViewModel.AvailableStatuses; // ส่ง List Status ไปให้ View

            return View(redemptions); // ส่ง List<AdminRedemptionViewModel> ไป
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRedemptionStatus(int redemptionId, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus) || !AdminRedemptionViewModel.AvailableStatuses.Contains(newStatus))
            {
                TempData["AdminError"] = "สถานะที่เลือกไม่ถูกต้อง";
                return RedirectToAction("ManageRedemptions");
            }

            // *** ต้องหา Redemption ด้วย RedemptionId (PK ใหม่) ***
            var redemptionToUpdate = await _context.Redemptions.FindAsync(redemptionId);

            if (redemptionToUpdate == null)
            {
                TempData["AdminError"] = $"ไม่พบ Redemption ID: {redemptionId}";
                return RedirectToAction("ManageRedemptions");
            }

            string oldStatus = redemptionToUpdate.Status;
            redemptionToUpdate.Status = newStatus;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin updated RedemptionId {RedemptionId} status from '{OldStatus}' to '{NewStatus}'", redemptionId, oldStatus, newStatus);
                TempData["AdminSuccess"] = $"อัปเดตสถานะ Redemption ID: {redemptionId} เป็น '{newStatus}' เรียบร้อยแล้ว";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating status for RedemptionId {RedemptionId}", redemptionId);
                TempData["AdminError"] = $"เกิดข้อผิดพลาดในการบันทึก Redemption ID: {redemptionId}";
            }

            // Redirect กลับไปหน้าเดิม (อาจจะต้องส่ง Filter กลับไปด้วย)
            return RedirectToAction("ManageRedemptions");
        }

        // --- จัดการ Stock ---
        [HttpGet]
        public async Task<IActionResult> ManageStock(int? filterBranchId) // รับ BranchId สำหรับ Filter (Optional)
        {
            _logger.LogInformation("Admin accessing ManageStock page. Filter BranchId: {BranchId}", filterBranchId);

            // ดึง Branch ทั้งหมดสำหรับ Dropdown Filter
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            ViewBag.FilterBranchId = filterBranchId;

            IQueryable<BranchProduct> query = _context.BranchProducts
                                                .Include(bp => bp.Branch)
                                                .Include(bp => bp.Product)
                                                    .ThenInclude(p => p.ProductImages);

            // Filter ตาม Branch (ถ้าเลือก)
            if (filterBranchId.HasValue && filterBranchId > 0)
            {
                query = query.Where(bp => bp.BranchId == filterBranchId.Value);
            }

            var stockItems = await query
                                .OrderBy(bp => bp.BranchId).ThenBy(bp => bp.ProductId)
                                .Select(bp => new AdminStockViewModel
                                {
                                    BranchId = bp.BranchId,
                                    BranchName = bp.Branch.Name,
                                    ProductId = bp.ProductId,
                                    ProductName = bp.Product.Name,
                                    ProductImageUrl = bp.Product.ProductImages
                                                                .OrderBy(img => img.ImageNo)
                                                                .FirstOrDefault() != null
                                                                ? bp.Product.ProductImages.OrderBy(img => img.ImageNo).First().Url
                                                                : "/images/placeholder.png",
                                    CurrentStock = bp.StockQuantity,
                                    QuantityToAdd = 0 // เริ่มต้น Input เป็น 0
                                })
                                .ToListAsync();

            return View(stockItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(int branchId, int productId, int quantityToAdd) // <<< เปลี่ยนชื่อ Action เป็น AddStock
        {
            // Validate Input เบื้องต้น (เหมือนเดิม)
            if (branchId <= 0 || productId <= 0) { /* ... Error ... */ return RedirectToAction("ManageStock"); }
            if (quantityToAdd < 0) { /* ... Error ... */ return RedirectToAction("ManageStock", new { filterBranchId = branchId }); }
            if (quantityToAdd == 0) { /* ... Info ... */ return RedirectToAction("ManageStock", new { filterBranchId = branchId }); }

            var branchProductToUpdate = await _context.BranchProducts
                                                  .FirstOrDefaultAsync(bp => bp.BranchId == branchId && bp.ProductId == productId);
            if (branchProductToUpdate == null) { /* ... Error ... */ return RedirectToAction("ManageStock", new { filterBranchId = branchId }); }

            int oldStock = branchProductToUpdate.StockQuantity;
            branchProductToUpdate.StockQuantity += quantityToAdd; // <<< เพิ่ม Stock

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin ADDED {Quantity} stock...", quantityToAdd /* ... rest of log ... */);
                TempData["AdminSuccess"] = $"เพิ่ม Stock จำนวน {quantityToAdd} ชิ้น สำเร็จ...";
            }
            catch (DbUpdateException ex) { /* ... Error ... */ }

            return RedirectToAction("ManageStock", new { filterBranchId = branchId });
        }


        // --- (เพิ่ม) ลด Stock ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStock(int branchId, int productId, int quantityToRemove) // <<< Action ใหม่ + Parameter ใหม่
        {
            // Validate Input เบื้องต้น
            if (branchId <= 0 || productId <= 0)
            {
                TempData["AdminError"] = "ข้อมูล Branch หรือ Product ไม่ถูกต้อง";
                return RedirectToAction("ManageStock");
            }
            if (quantityToRemove <= 0) // <<< ต้องกรอกค่ามากกว่า 0 ที่จะลบ
            {
                TempData["AdminError"] = "จำนวนที่ลดต้องมากกว่า 0";
                return RedirectToAction("ManageStock", new { filterBranchId = branchId });
            }

            // หา BranchProduct (ใช้ Tracking)
            var branchProductToUpdate = await _context.BranchProducts
                                                  .FirstOrDefaultAsync(bp => bp.BranchId == branchId && bp.ProductId == productId);

            if (branchProductToUpdate == null)
            {
                TempData["AdminError"] = $"ไม่พบข้อมูล Stock สำหรับ BranchId: {branchId}, ProductId: {productId}";
                return RedirectToAction("ManageStock", new { filterBranchId = branchId });
            }

            // ***** (แก้ไข) ตรวจสอบ Stock ก่อนลบ *****
            if (branchProductToUpdate.StockQuantity < quantityToRemove)
            {
                TempData["AdminError"] = $"ไม่สามารถลด Stock ได้ สต็อกปัจจุบัน ({branchProductToUpdate.StockQuantity}) น้อยกว่าจำนวนที่ต้องการลด ({quantityToRemove})";
                return RedirectToAction("ManageStock", new { filterBranchId = branchId });
            }
            // ***** จบการตรวจสอบ *****

            int oldStock = branchProductToUpdate.StockQuantity;
            branchProductToUpdate.StockQuantity -= quantityToRemove; // <<<--- แก้ไขเป็นการลบ Stock

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin REMOVED {Quantity} stock for Product {ProductId} at Branch {BranchId}. Old Stock: {OldStock}, New Stock: {NewStock}",
                    quantityToRemove, productId, branchId, oldStock, branchProductToUpdate.StockQuantity); // <<< แก้ Log Message
                TempData["AdminSuccess"] = $"ลด Stock จำนวน {quantityToRemove} ชิ้น สำหรับ Product ID: {productId} ที่ Branch ID: {branchId} สำเร็จ (คงเหลือ {branchProductToUpdate.StockQuantity})"; // <<< แก้ TempData Message
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating stock for BranchId {BranchId}, ProductId {ProductId} during removal", branchId, productId);
                TempData["AdminError"] = $"เกิดข้อผิดพลาดในการบันทึก Stock";
            }

            // Redirect กลับไปหน้าเดิม พร้อม Filter สาขาเดิม
            return RedirectToAction("ManageStock", new { filterBranchId = branchId });
        }
    }
}