// ใน Services/BookingService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyProject.Data;
using MyProject.Models;
using MyProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Services
{
    public class BookingService : IBookingService
    {
        private readonly MyBookstoreDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(MyBookstoreDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public decimal CalculateOptimizedPrice(int numberOfDays, Product product)
        {
            // (โค้ด Helper CalculateOptimizedPriceInternal เดิมของคุณ)
            if (numberOfDays <= 0 || product == null) return 0;

            decimal basePrice = 0;
            int remainingDays = numberOfDays;
            int months = 0, weeks = 0, days = 0;
            const int daysInMonth = 30; // สมมติฐาน
            const int daysInWeek = 7;

            // คำนวณเดือน
            if (product.PricePerMonth > 0 && remainingDays >= daysInMonth)
            {
                months = (int)Math.Floor((decimal)remainingDays / daysInMonth);
                basePrice += months * product.PricePerMonth;
                remainingDays %= daysInMonth;
            }
            // คำนวณสัปดาห์
            if (product.PricePerWeek > 0 && remainingDays >= daysInWeek)
            {
                if (product.PricePerWeek < product.PricePerDay * daysInWeek)
                {
                    weeks = (int)Math.Floor((decimal)remainingDays / daysInWeek);
                    basePrice += weeks * product.PricePerWeek;
                    remainingDays %= daysInWeek;
                }
            }
            // คำนวณวันที่เหลือ
            if (remainingDays > 0)
            {
                days = remainingDays;
                basePrice += days * product.PricePerDay;
            }
            return basePrice;
        }

        public async Task<BookingCreationResult> CreateBookingAsync(BookingInputViewModel model, string userEmail)
        {
            // --- 2. Get User ---
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null)
            {
                _logger.LogWarning("Booking creation failed: User not found for email {Email}", userEmail);
                return new BookingCreationResult { Success = false, ErrorMessage = "ไม่พบข้อมูลผู้ใช้" };
            }
            int userId = currentUser.UserId;

            // --- 3. Get Product ---
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == model.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Booking creation failed: Product not found {ProductId}", model.ProductId);
                return new BookingCreationResult { Success = false, ErrorMessage = "ไม่พบข้อมูลรถ" };
            }

            // --- 4. Determine Branch & Check Stock ---
            int branchIdToCheck = model.BranchId;
            // *** ต้องใช้ Tracking (ห้าม AsNoTracking) เพราะจะ Update Stock ***
            var branchProduct = await _context.BranchProducts
                .FirstOrDefaultAsync(bp => bp.BranchId == branchIdToCheck && bp.ProductId == model.ProductId);

            if (branchProduct == null)
            {
                _logger.LogWarning("Booking failed: BranchProduct mapping not found for Product {ProductId}, Branch {BranchId}", model.ProductId, branchIdToCheck);
                var branchName = await _context.Branches.Where(b => b.BranchId == branchIdToCheck).Select(b => b.Name).FirstOrDefaultAsync();
                return new BookingCreationResult { Success = false, ErrorMessage = $"ขออภัย รถ {product.Name} ไม่มีจำหน่ายในสาขา {branchName ?? "ที่เลือก"}" };
            }
            if (branchProduct.StockQuantity <= 0)
            {
                _logger.LogWarning("Booking failed: Product {ProductId} out of stock at Branch {BranchId}", model.ProductId, branchIdToCheck);
                var branchName = await _context.Branches.Where(b => b.BranchId == branchIdToCheck).Select(b => b.Name).FirstOrDefaultAsync();
                return new BookingCreationResult { Success = false, ErrorMessage = $"ขออภัย รถ {product.Name} หมดสต็อกในสาขา {branchName ?? "ที่เลือก"}" };
            }

            // --- 5. Validate Discount ---
            Discount? appliedDiscount = null;
            if (!string.IsNullOrWhiteSpace(model.DiscountCode))
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                appliedDiscount = await _context.Discounts.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Code == model.DiscountCode && d.ExpiryDate >= today);
                if (appliedDiscount == null)
                {
                    _logger.LogWarning("Booking failed: Invalid discount code {Code}", model.DiscountCode);
                    return new BookingCreationResult { Success = false, ErrorMessage = $"รหัสส่วนลด '{model.DiscountCode}' ไม่ถูกต้องหรือหมดอายุ" };
                }
            }

            // --- 6. Calculate Price ---
            TimeSpan duration = model.ReturnDate - model.PickupDate;
            int numberOfDays = duration.Days + 1;
            decimal basePrice = CalculateOptimizedPrice(numberOfDays, product); // <<< เรียกใช้ Helper

            // --- 7. Calculate Points ---
            int calculatedPoints = (int)Math.Ceiling(basePrice / 10);

            // --- 8. Create Order ---
            var order = new Order
            {
                UserId = userId,
                ProductId = model.ProductId,
                BranchId = branchIdToCheck,
                DateReceipt = DateOnly.FromDateTime(model.PickupDate),
                DateReturn = DateOnly.FromDateTime(model.ReturnDate),
                Price = basePrice,
                Point = calculatedPoints,
                DiscountId = appliedDiscount?.DiscountId
            };
            _context.Orders.Add(order);

            // --- 9. Update Stock ---
            branchProduct.StockQuantity -= 1;
            _logger.LogInformation("Stock decremented for Product {ProductId} at Branch {BranchId}. New stock: {Stock}", product.ProductId, branchIdToCheck, branchProduct.StockQuantity);

            // --- 10. Save Changes (ทั้ง Order และ Stock) ---
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Booking created successfully. OrderId {OrderId}, UserId {UserId}", order.OrderId, userId);
                // คืนค่า OrderId ที่สร้างสำเร็จ
                return new BookingCreationResult { Success = true, CreatedOrderId = order.OrderId };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException during booking creation for Product {ProductId}", model.ProductId);
                return new BookingCreationResult { Success = false, ErrorMessage = "เกิดข้อผิดพลาดในการบันทึกข้อมูลการจอง" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception during booking creation for Product {ProductId}", model.ProductId);
                return new BookingCreationResult { Success = false, ErrorMessage = "เกิดข้อผิดพลาดไม่คาดคิด" };
            }
        }

        public async Task<BookingDeletionResult> DeleteBookingAsync(int orderId, string userEmail)
        {
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null)
            {
                return new BookingDeletionResult { Success = false, ErrorMessage = "ไม่พบผู้ใช้" };
            }
            int currentUserId = currentUser.UserId;

            _logger.LogInformation("Service attempting to delete OrderId {OrderId} for UserId {UserId}", orderId, currentUserId);

            try
            {
                // (ย้ายโค้ดจาก Controller มา)
                // *** ห้ามใช้ AsNoTracking() เพราะจะลบ ***
                var orderToDelete = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == currentUserId);

                if (orderToDelete == null)
                {
                    _logger.LogWarning("Service delete failed: Order {OrderId} not found or access denied for UserId {UserId}", orderId, currentUserId);
                    return new BookingDeletionResult { Success = false, ErrorMessage = "ไม่พบรายการจองที่จะลบ หรือคุณไม่มีสิทธิ์" };
                }

                // (ย้ายโค้ดคืน Stock มา)
                var branchProductToUpdate = await _context.BranchProducts
                    .FirstOrDefaultAsync(bp => bp.BranchId == orderToDelete.BranchId && bp.ProductId == orderToDelete.ProductId);

                if (branchProductToUpdate != null)
                {
                    branchProductToUpdate.StockQuantity += 1;
                    _logger.LogInformation("Service incremented stock for Product {ProdId} at Branch {BranchId} due to deletion.",
                        orderToDelete.ProductId, orderToDelete.BranchId);
                }
                else
                {
                    _logger.LogWarning("Service could not find BranchProduct to restock for deleted Order {OrderId}.", orderToDelete.OrderId);
                }

                // ทำการลบ
                _context.Orders.Remove(orderToDelete);
                await _context.SaveChangesAsync(); // <<< SaveChanges จะลบ Order และ อัปเดต Stock พร้อมกัน

                _logger.LogInformation("Service successfully deleted OrderId {OrderId}", orderId);
                return new BookingDeletionResult { Success = true };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Service DbUpdateException deleting OrderId {OrderId}", orderId);
                return new BookingDeletionResult { Success = false, ErrorMessage = "เกิดข้อผิดพลาดฐานข้อมูลขณะลบ" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service Error deleting OrderId {OrderId}", orderId);
                return new BookingDeletionResult { Success = false, ErrorMessage = "เกิดข้อผิดพลาดขณะลบรายการจอง" };
            }
        }


        // ***** (เพิ่ม) Logic การดึงประวัติการเช่า *****
        public async Task<List<RentalHistoryItemViewModel>> GetRentalHistoryAsync(string userEmail)
        {
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null)
            {
                _logger.LogWarning("GetRentalHistoryAsync failed: User not found for email {Email}", userEmail);
                return new List<RentalHistoryItemViewModel>(); // คืนค่า List ว่าง
            }
            int currentUserId = currentUser.UserId;

            _logger.LogInformation("Service fetching rental history for UserId {UserId}", currentUserId);

            // (ย้าย Query ที่ซับซ้อนมาจาก Controller)
            var rentalHistory = await _context.Orders
                .Where(o => o.UserId == currentUserId)
                .Include(o => o.Product)
                    .ThenInclude(p => p!.ProductImages) // <<< Include รูปภาพ
                .Include(o => o.Payment)
                .Where(o => o.Payment != null && (o.Payment.Status == "In progress" || o.Payment.Status == "Completed"))
                .OrderByDescending(o => o.Payment!.Date)
                .Select(o => new RentalHistoryItemViewModel
                {
                    OrderId = o.OrderId,
                    ProductName = o.Product != null ? o.Product.Name : "N/A",
                    // (ดึงรูปปกแบบปลอดภัย)
                    ProductImageUrl = (o.Product != null && o.Product.ProductImages.Any()) // <<< 1. เช็คว่ามี Product และมีรูป
                                      ? o.Product.ProductImages.OrderBy(img => img.ImageNo).Select(img => img.Url).FirstOrDefault() // <<< 2. Select Url ออกมาก่อน แล้วค่อย FirstOrDefault
                                      : "/images/placeholder.png",
                    PickupDate = o.DateReceipt,
                    ReturnDate = o.DateReturn,
                    PaymentDate = o.Payment!.Date,
                    PaymentStatus = o.Payment.Status,
                    AmountPaid = o.Payment.Amount
                })
                .AsNoTracking()
                .ToListAsync();

            return rentalHistory;
        }
    }
}