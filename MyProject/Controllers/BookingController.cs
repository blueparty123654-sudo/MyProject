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
    public class BookingController : Controller
    {
        private readonly MyBookstoreDbContext _context;
        private readonly ILogger<BookingController> _logger;

        public BookingController(MyBookstoreDbContext context, ILogger<BookingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet] // ระบุว่าเป็น GET Request
        [AllowAnonymous]
        public async Task<IActionResult> Create(int productId, int branchId) // <--- เปลี่ยนชื่อ Action และ Parameter เป็น productId
        {
            // --- 1. Increment View Count ---
            bool skipViewCount = TempData["SkipViewCount"] as bool? ?? false;

            if (User.Identity != null && User.Identity.IsAuthenticated && !skipViewCount) // <--- เพิ่ม !skipViewCount ที่นี่
            {
                try
                {
                    var userEmail = User.FindFirstValue(ClaimTypes.Email);
                    var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);

                    if (currentUser != null)
                    {
                        int userId = currentUser.UserId;
                        string currentMonthYear = DateTime.Now.ToString("yyyy-MM");

                        // ดึง Record มา Track (ถ้าเจอ)
                        var favoriteRecord = await _context.Favorites
                            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                        if (favoriteRecord != null) // --- ถ้าเจอ Record ของ User/Product นี้ ---
                        {
                            // เช็คว่า MonthYear ตรงกับเดือนปัจจุบันหรือไม่
                            if (favoriteRecord.MonthYear == currentMonthYear)
                            {
                                // เดือนเดียวกัน -> บวก ViewCount
                                favoriteRecord.ViewCount += 1;
                                _logger.LogInformation("Incremented favorite view count...");
                            }
                            else
                            {
                                // คนละเดือน -> อัปเดต MonthYear และ Reset ViewCount เป็น 1
                                favoriteRecord.MonthYear = currentMonthYear;
                                favoriteRecord.ViewCount = 1;
                                _logger.LogInformation("Reset favorite view count for new month...");
                            }
                        }
                        else // --- ถ้าไม่เจอ Record ของ User/Product นี้เลย ---
                        {
                            // สร้าง Record ใหม่
                            _context.Favorites.Add(new Favorite
                            {
                                UserId = userId,
                                ProductId = productId,
                                MonthYear = currentMonthYear, // ใส่ MonthYear ปัจจุบัน
                                ViewCount = 1             // เริ่มนับ 1
                            });
                            _logger.LogInformation("Added new favorite record...");
                        }
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Updated/Added favorite view count for UserId={UserId}, ProductId={ProductId}", userId, productId); // Log เพิ่มเติม
                    }
                    else { _logger.LogWarning("Authenticated user not found in DB (Email: {UserEmail}) for view count.", userEmail); }
                }
                catch (DbUpdateException ex) // <--- ใช้ DbUpdateException
                {
                    _logger.LogError(ex, "Error updating favorite view count for ProductId {ProductId}. Inner: {InnerMessage}", productId, ex.InnerException?.Message);
                }
                catch (Exception ex) { _logger.LogError(ex, "Unexpected error updating favorite view count for ProductId {ProductId}", productId); }
            }
            else if (skipViewCount)
            {
                _logger.LogInformation("Skipping favorite view count update due to redirect for ProductId {ProductId}", productId);
                TempData.Remove("SkipViewCount"); // ใช้แล้วลบออก
            }
            else
            {
                _logger.LogInformation("User not authenticated. Skipping favorite view count update for ProductId {ProductId}", productId);
            }
            // --- จบ View Count ---


            // --- 2. Fetch Product Details ---
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId); // <--- เปลี่ยน id เป็น productId

            if (product == null) { return NotFound($"Product with ID {productId} not found."); } // <--- เพิ่ม Message ให้ NotFound

            var selectedBranch = await _context.Branches
                                .AsNoTracking()
                                .Select(b => new { b.BranchId, b.Name })
                                .FirstOrDefaultAsync(b => b.BranchId == branchId);

            // --- 3. Create ViewModel (คัดลอกมา) ---
            // *** สำคัญ: ตรวจสอบว่า ProductDetailsViewModel มี BranchId, BranchName แล้ว ***
            var viewModel = new ProductDetailsViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                PricePerDay = product.PricePerDay,
                PricePerWeek = product.PricePerWeek,
                PricePerMonth = product.PricePerMonth,
                ImageUrl = product.ImageUrl,
                ImageUrl2 = product.ImageUrl2,
                GearType = product.GearType,
                Engine = product.Engine,
                CoolingSystem = product.CoolingSystem,
                StartingSystem = product.StartingSystem,
                FuelType = product.FuelType,
                FuelDispensing = product.FuelDispensing,
                FuelTankCapacity = product.FuelTankCapacity,
                BrakeSystem = product.BrakeSystem,
                Suspension = product.Suspension,
                TireSize = product.TireSize,
                Dimensions = product.Dimensions,
                VehicleWeight = product.VehicleWeight,
                BranchId = selectedBranch?.BranchId ?? 0, // ถ้าหา Branch ไม่เจอ ใช้ 0 หรือจัดการ Error
                BranchName = selectedBranch?.Name ?? "ไม่พบสาขา"
            };

            // --- (แก้ไข) ระบุชื่อ View ให้ถูกต้อง ---
            return View("CreateBooking", viewModel); // <--- ต้องระบุชื่อ View "CreateBooking"
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingInputViewModel model)
        {
            _logger.LogInformation("Booking Create POST received for ProductId: {ProductId}", model.ProductId);

            // --- 1. Basic & Date Validation ---
            if (!ModelState.IsValid || model.ReturnDate <= model.PickupDate)
            {
                TempData["BookingError"] = !ModelState.IsValid ? "ข้อมูลไม่ครบถ้วน" : "วันที่คืนรถต้องอยู่หลังวันที่รับรถ";
                TempData["SkipViewCount"] = true;
                _logger.LogWarning("Booking validation failed for ProductId: {ProductId}", model.ProductId);
                return RedirectToAction("Create", "Booking", new { productId = model.ProductId, branchId = model.BranchId });
            }

            if (model.PickupDate.Date < DateTime.Today)
            {
                TempData["BookingError"] = "วันที่รับรถต้องไม่ใช่วันในอดีต";
                TempData["SkipViewCount"] = true;
                _logger.LogWarning("Booking failed: PickupDate is in the past for ProductId: {ProductId}", model.ProductId);
                return RedirectToAction("Create", "Booking", new { productId = model.ProductId, branchId = model.BranchId });
            }

            // --- 2. Get User ---
            var userEmail = User.FindFirstValue(ClaimTypes.Email); // หรือ ClaimTypes.NameIdentifier ถ้าเก็บ ID
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail); // *** ตรวจสอบ Property Email/UserId ***
            if (currentUser == null)
            {
                TempData["BookingError"] = "ไม่พบข้อมูลผู้ใช้";
                TempData["SkipViewCount"] = true;
                _logger.LogError("Booking failed: User not found for email: {Email}", userEmail);
                return RedirectToAction("Create", "Booking", new { productId = model.ProductId, branchId = model.BranchId });
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
            int branchIdToCheck = model.BranchId;
            _logger.LogInformation("Booking attempt for ProductId: {ProductId}, BranchId: {BranchId}", model.ProductId, branchIdToCheck);

            // --- (แก้ไข) Query Branch เพื่อใช้ชื่อใน Error Message ---
            var branchForBooking = await _context.Branches.AsNoTracking().Select(b => new { b.BranchId, b.Name }).FirstOrDefaultAsync(b => b.BranchId == branchIdToCheck);
            if (branchForBooking == null)
            {
                TempData["BookingError"] = "ไม่พบข้อมูลสาขาที่ระบุ";
                _logger.LogError("Booking failed: BranchId {BranchId} not found.", branchIdToCheck);
                return RedirectToAction("Create", "Booking", new { productId = model.ProductId, branchId = model.BranchId });
            }

            var branchProduct = await _context.BranchProducts
                .FirstOrDefaultAsync(bp => bp.BranchId == branchIdToCheck && bp.ProductId == model.ProductId); // <-- ใช้ FirstOrDefaultAsync

            if (branchProduct == null || branchProduct.StockQuantity <= 0)
            {
                TempData["BookingError"] = $"ขออภัย รถ {product.Name} {(branchProduct == null ? "ไม่มีจำหน่าย" : "หมด")} ในสาขา {branchForBooking.Name} ชั่วคราว";
                TempData["SkipViewCount"] = true;
                _logger.LogWarning("Booking failed: Product {ProductId} not found or out of stock at BranchId {BranchId}", model.ProductId, branchIdToCheck);
                // ***** (แก้ไข) เพิ่ม branchId *****
                return RedirectToAction("Create", "Booking", new { productId = model.ProductId, branchId = model.BranchId });
            }

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
                    TempData["SkipViewCount"] = true;
                    _logger.LogWarning("Booking failed: Invalid discount code {Code} submitted.", model.DiscountCode);
                    return RedirectToAction("Create", "Booking", new { productId = model.ProductId, branchId = model.BranchId });
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
            int calculatedPoints = (int)Math.Ceiling(basePrice / 10);

            _logger.LogInformation("Calculated Base Price: {BasePrice}, Points: {Points}, Days: {Days}", basePrice, calculatedPoints, numberOfDays);

            // --- 7. Create Order ---
            var order = new Order
            {
                UserId = userId,
                ProductId = model.ProductId,
                BranchId = branchIdToCheck,
                DateReceipt = DateOnly.FromDateTime(model.PickupDate),
                DateReturn = DateOnly.FromDateTime(model.ReturnDate),
                Price = basePrice,              // *** ใช้ราคาก่อนหักส่วนลด ***
                Point = calculatedPoints,       // *** ใส่ Point ที่คำนวณแล้ว ***
                DiscountId = appliedDiscount?.DiscountId // เก็บ ID ส่วนลด (ถ้ามี)
                                                         // ตรวจสอบ Property อื่นๆ ที่จำเป็นใน Model Order ของคุณ
            };
            _context.Orders.Add(order);


            // --- 8. Update Stock ---
            branchProduct.StockQuantity -= 1;
            _logger.LogInformation("Decremented stock for Product {ProdId} at Branch {BranchId}. New Stock: {Stock}",
                model.ProductId, branchIdToCheck, branchProduct.StockQuantity);



            // --- 9. Save Changes ---
            int savedOrderId = 0;
            try
            {
                await _context.SaveChangesAsync();
                savedOrderId = order.OrderId; // <-- ดึง OrderId หลัง Save สำเร็จ
                _logger.LogInformation("SaveChangesAsync successful. OrderId: {OrderId}", savedOrderId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving booking changes for ProductId {ProductId}", model.ProductId);
                TempData["BookingError"] = "เกิดข้อผิดพลาดในการบันทึกข้อมูลการจอง";
                TempData["SkipViewCount"] = true;

                return RedirectToAction("Create", "Booking", new { productId = model.ProductId, branchId = model.BranchId });
            }

            _logger.LogInformation("Booking successful for ProductId: {ProductId}, UserId: {UserId}, OrderId: {OrderId}", model.ProductId, userId, savedOrderId);
            return RedirectToAction("Details", "Booking", new { orderId = savedOrderId });
        }

        [HttpPost]
        // [ValidateAntiForgeryToken] // อาจจะต้องจัดการ Token ต่างหากสำหรับ AJAX
        public async Task<IActionResult> ValidateDiscount([FromBody] DiscountValidationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
            {
                return Json(new { isValid = false, message = "กรุณากรอกรหัส" });
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            // --- ส่วนที่เช็คฐานข้อมูล ---
            var discount = await _context.Discounts.AsNoTracking() // <--- 1. ระบุตาราง Discounts
                                    .FirstOrDefaultAsync(d =>
                                        d.Code == request.Code && // <--- 2. เช็คคอลัมน์ Code
                                        d.Date >= today);         // <--- 3. เช็คคอลัมน์ Date (วันหมดอายุ)
                                                                  // --- สิ้นสุดการเช็ค ---

            if (discount != null) // ถ้าเจอโค้ดที่ตรงกันและยังไม่หมดอายุ
            {
                // ส่ง Rate กลับไปด้วย
                return Json(new { isValid = true, message = $"ส่วนลด {discount.Rate}% ใช้ได้!", rate = discount.Rate });
            }
            else // ถ้าไม่เจอ หรือหมดอายุ
            {
                return Json(new { isValid = false, message = "รหัสส่วนลดไม่ถูกต้อง หรือหมดอายุแล้ว" });
            }
        }


        [HttpGet] // ระบุว่าเป็น GET
        [Authorize] // ต้อง Login
        public async Task<IActionResult> Details(int orderId) // รับ OrderId
        {
            // 1. ดึง UserId ของคนที่ Login อยู่
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null) return Challenge(); // Redirect ไป Login
            int currentUserId = currentUser.UserId;

            // 2. Query Order พร้อมข้อมูลที่เกี่ยวข้อง
            var order = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Branch)
                .Include(o => o.Discount)
                .Include(o => o.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == currentUserId); // <-- เช็ค UserId ด้วย

            // 3. ตรวจสอบว่าเจอ Order หรือไม่
            if (order == null)
            {
                _logger.LogWarning("Order not found or access denied for OrderId {OrderId}, UserId {UserId}", orderId, currentUserId);
                return NotFound("ไม่พบรายการจองที่คุณต้องการ หรือคุณไม่มีสิทธิ์เข้าถึง");
            }

            // 4. คำนวณต่างๆ
            int numberOfDays = (order.DateReturn.DayNumber - order.DateReceipt.DayNumber) + 1;
            decimal originalPrice = order.Price;
            decimal discountAmount = 0;
            decimal? discountRate = null;
            string? discountCode = null;
            if (order.Discount != null)
            {
                discountRate = order.Discount.Rate;
                discountCode = order.Discount.Code;
                if (discountRate > 0)
                {
                    discountAmount = Math.Round(originalPrice * (discountRate.Value / 100m), 2);
                }
            }
            decimal finalPrice = originalPrice - discountAmount;

            // 5. สร้าง ViewModel (BookingDetailsViewModel)
            var viewModel = new BookingDetailsViewModel
            {
                OrderId = order.OrderId,
                ProductName = order.Product?.Name,
                ProductImageUrl = order.Product?.ImageUrl,
                BranchName = order.Branch?.Name,
                PickupDate = order.DateReceipt,
                ReturnDate = order.DateReturn,
                NumberOfDays = numberOfDays,
                OriginalPrice = originalPrice,
                DiscountCode = discountCode,
                DiscountRate = discountRate,
                DiscountAmount = discountAmount,
                FinalPrice = finalPrice,
                PointsEarned = order.Point,
                UserName = order.User?.Name
            };

            // 6. ส่ง ViewModel ไปให้ View ชื่อ "Detail"
            return View("Detail", viewModel); // <--- ระบุชื่อ View "Detail"
        }

        // Class รับค่า JSON จาก AJAX (ต้องมี)
        public class DiscountValidationRequest
        {
            public string? Code { get; set; }
        }

        [HttpGet] // ระบุว่าเป็น GET
        [Authorize] // ต้อง Login
        public async Task<IActionResult> Edit(int orderId)
        {
            // 1. ดึง UserId
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null) return Challenge();
            int currentUserId = currentUser.UserId;

            // 2. ค้นหา Order เดิม (Include ข้อมูลที่จำเป็น)
            // *** สำคัญ: ต้องไม่ใช้ AsNoTracking() เพราะเราอาจจะต้อง Update ใน POST ***
            // แต่สำหรับ GET ใช้ AsNoTracking() ก็ได้ ถ้ามั่นใจว่า POST จะดึงมาใหม่
            var order = await _context.Orders
                .Include(o => o.Product) // เอาชื่อ Product
                .Include(o => o.Branch) // เอาชื่อ Branch
                .Include(o => o.Discount) // เอา Discount Code เดิม
                .AsNoTracking() // ใช้ AsNoTracking() สำหรับ GET เพื่อ Performance
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == currentUserId); // <-- เช็ค UserId

            // 3. ตรวจสอบว่าเจอ Order หรือไม่
            if (order == null)
            {
                _logger.LogWarning("Edit GET failed: Order not found or access denied for OrderId {OrderId}, UserId {UserId}", orderId, currentUserId);
                // อาจจะ Redirect ไปหน้าแสดง Error หรือ NotFound()
                TempData["ErrorMessage"] = "ไม่พบรายการจองที่คุณต้องการแก้ไข หรือคุณไม่มีสิทธิ์";
                return RedirectToAction("Index", "Home"); // หรือหน้า MyBookings ถ้ามี
            }

            // 4. สร้าง ViewModel สำหรับหน้า Edit
            var viewModel = new BookingEditViewModel
            {
                OrderId = order.OrderId,
                ProductId = order.ProductId, // เก็บ ProductId ไว้
                ProductName = order.Product?.Name,
                BranchName = order.Branch?.Name,
                // *** แปลง DateOnly เป็น DateTime สำหรับ Input ***
                PickupDate = order.DateReceipt.ToDateTime(TimeOnly.MinValue),
                ReturnDate = order.DateReturn.ToDateTime(TimeOnly.MinValue),
                DiscountCode = order.Discount?.Code // แสดงโค้ดเดิม (ถ้ามี)
            };

            if (order.Product != null)
            {
                ViewBag.ProductPrices = new Tuple<decimal, decimal, decimal>(
                    order.Product.PricePerDay,
                    order.Product.PricePerWeek,
                    order.Product.PricePerMonth
                );
                _logger.LogInformation("Sending product prices to Edit view via ViewBag."); // Log เพิ่มเติม
            }
            else
            {
                _logger.LogWarning("Product not found when trying to send prices to Edit view for OrderId {OrderId}", orderId);
                // อาจจะส่งราคาเป็น 0 ไป หรือจัดการ Error แบบอื่น
                ViewBag.ProductPrices = new Tuple<decimal, decimal, decimal>(0, 0, 0);
            }

            // 5. ส่งไป View ใหม่ชื่อ "Edit"
            return View("Edit", viewModel); // <-- ระบุชื่อ View "Edit"
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // ป้องกัน CSRF
        [Authorize] // ต้อง Login
        public async Task<IActionResult> Edit(BookingEditViewModel model) // รับ ViewModel
        {
            _logger.LogInformation("Booking Edit POST received for OrderId: {OrderId}", model.OrderId);

            // 1. Validate ViewModel & Dates
            if (!ModelState.IsValid || model.ReturnDate <= model.PickupDate)
            {
                TempData["EditBookingError"] = !ModelState.IsValid ? "ข้อมูลไม่ครบถ้วน" : "วันที่คืนรถต้องอยู่หลังวันที่รับรถ";
                _logger.LogWarning("Edit validation failed (Model State/Date Order) for OrderId: {OrderId}", model.OrderId);
                await PopulateViewModelForEditError(model);
                // *** ต้องส่งข้อมูล ProductName, BranchName กลับไปให้ View Edit ด้วย ***
                model.ProductName = (await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == model.ProductId))?.Name;
                // model.BranchName = ... // อาจจะต้อง Query ใหม่
                return View("Edit", model); // กลับไปหน้า Edit พร้อม Error
            }
            if (model.PickupDate.Date < DateTime.Today)
            {
                TempData["EditBookingError"] = "วันที่รับรถต้องไม่ใช่วันในอดีต";
                _logger.LogWarning("Edit validation failed (Past Date) for OrderId: {OrderId}", model.OrderId);
                await PopulateViewModelForEditError(model);
                // ... Populate ProductName/BranchName ...
                return View("Edit", model);
            }

            // 2. ดึง UserId และ Order เดิม (*** ห้ามใช้ AsNoTracking() ***)
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null) return Challenge();
            int currentUserId = currentUser.UserId;

            var order = await _context.Orders
                .Include(o => o.Discount) // Include Discount เพื่อเปรียบเทียบ
                .FirstOrDefaultAsync(o => o.OrderId == model.OrderId && o.UserId == currentUserId);

            if (order == null)
            {
                _logger.LogWarning("Edit POST failed: Order not found or access denied...");
                TempData["ErrorMessage"] = "ไม่พบรายการจองที่คุณต้องการแก้ไข หรือคุณไม่มีสิทธิ์";
                return RedirectToAction("Index", "Home"); // หรือ MyBookings
            }

            // 3. ตรวจสอบ Stock (*** ข้ามไปก่อน ***)
            // TODO: Implement stock/conflict check if dates changed significantly

            // 4. Validate Discount Code (ถ้ามีการเปลี่ยนแปลง)
            Discount? newAppliedDiscount = null; // เก็บ Discount ใหม่ (ถ้ามี)
            int? newDiscountId = order.DiscountId; // ใช้ค่าเดิมเป็น Default
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            bool discountError = false;
            string discountErrorMessage = "";

            if (!string.IsNullOrWhiteSpace(model.DiscountCode))
            {
                if (order.Discount?.Code != model.DiscountCode) // โค้ดเปลี่ยน หรือ เดิมไม่มี
                {
                    newAppliedDiscount = await _context.Discounts.AsNoTracking().FirstOrDefaultAsync(d => d.Code == model.DiscountCode && d.Date >= today);
                    if (newAppliedDiscount == null) { discountError = true; discountErrorMessage = $"รหัส '{model.DiscountCode}' ไม่ถูกต้อง/หมดอายุ"; }
                    else { newDiscountId = newAppliedDiscount.DiscountId; }
                }
                else
                { // โค้ดเหมือนเดิม, เช็คว่าหมดอายุหรือยัง
                    newDiscountId = order.DiscountId;
                    if (order.Discount != null && order.Discount.Date < today)
                    {
                        discountError = true; discountErrorMessage = $"รหัสเดิม '{model.DiscountCode}' หมดอายุแล้ว";
                        newDiscountId = null; // ถ้าหมดอายุ ให้เอาออก
                    }
                }
            }
            else { newDiscountId = null; } // ลบส่วนลด

            if (discountError)
            {
                TempData["EditBookingError"] = discountErrorMessage;
                _logger.LogWarning("Edit validation failed (Discount)...");
                await PopulateViewModelForEditError(model);
                // ... Populate ProductName/BranchName ...
                return View("Edit", model);
            }

            // 5. คำนวณราคาและ Point ใหม่
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == order.ProductId);
            if (product == null) { /* Handle error */ return NotFound("Product not found"); }

            TimeSpan newDuration = model.ReturnDate - model.PickupDate;
            int newNumberOfDays = newDuration.Days + 1;
            // *** เรียกใช้ Helper Function ที่สร้างไว้แล้ว ***
            decimal newBasePrice = CalculateOptimizedPriceInternal(newNumberOfDays, product);
            int newCalculatedPoints = (int)Math.Ceiling(newBasePrice / 10);

            // 6. อัปเดตข้อมูลใน Order Entity ที่ดึงมา
            order.DateReceipt = DateOnly.FromDateTime(model.PickupDate);
            order.DateReturn = DateOnly.FromDateTime(model.ReturnDate);
            order.Price = newBasePrice;
            order.Point = newCalculatedPoints;
            order.DiscountId = newDiscountId;

            _logger.LogInformation("Updating OrderId {OrderId}. New Dates: {Pickup} to {Return}, Price: {Price}",
                order.OrderId, order.DateReceipt, order.DateReturn, order.Price);

            // 7. Save Changes
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving edited order changes for OrderId {OrderId}", model.OrderId);
                TempData["EditBookingError"] = "เกิดข้อผิดพลาดในการบันทึกการแก้ไข";
                await PopulateViewModelForEditError(model);
                // ... Populate ProductName/BranchName ...
                return View("Edit", model);
            }

            // 8. Redirect กลับไปหน้า Details
            TempData["BookingUpdateSuccess"] = $"แก้ไขรายการจอง #{order.OrderId} เรียบร้อยแล้ว!"; // ส่ง Message ไปหน้า Details (ถ้าต้องการ)
            return RedirectToAction("Details", "Booking", new { orderId = order.OrderId });
        }

        private async Task PopulateViewModelForEditError(BookingEditViewModel model)
        {
            if (string.IsNullOrEmpty(model.ProductName))
            {
                model.ProductName = (await _context.Products.AsNoTracking().Select(p => new { p.ProductId, p.Name }).FirstOrDefaultAsync(p => p.ProductId == model.ProductId))?.Name;
            }
            if (string.IsNullOrEmpty(model.BranchName))
            {
                // เราอาจจะต้อง Query BranchId จาก Order เดิมก่อน ถ้า model ไม่มี BranchName มา
                var originalOrderBranchId = await _context.Orders.Where(o => o.OrderId == model.OrderId).Select(o => o.BranchId).FirstOrDefaultAsync();
                if (originalOrderBranchId > 0)
                {
                    model.BranchName = (await _context.Branches.AsNoTracking().Select(b => new { b.BranchId, b.Name }).FirstOrDefaultAsync(b => b.BranchId == originalOrderBranchId))?.Name;
                }
                model.BranchName ??= "N/A"; // ถ้ายังหาไม่ได้
            }
        }

        private decimal CalculateOptimizedPriceInternal(int numberOfDays, Product product)
        {
            // เช็คเบื้องต้น
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

        [HttpGet]
        [Authorize] // ต้อง Login
        public async Task<IActionResult> GetUserBookings()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null) { return Unauthorized(new { message = "ไม่พบผู้ใช้" }); }
            int currentUserId = currentUser.UserId;

            try
            {
                var bookings = await _context.Orders
                    .Where(o => o.UserId == currentUserId)
                    .Include(o => o.Product) // เอาชื่อและรูป Product
                    .OrderByDescending(o => o.OrderId) // เรียงตามล่าสุดก่อน
                    .Select(o => new BookingHistoryItemViewModel
                    {
                        OrderId = o.OrderId,
                        ProductName = o.Product.Name,
                        ProductImageUrl = o.Product.ImageUrl,
                        PickupDate = o.DateReceipt,
                        ReturnDate = o.DateReturn,
                        FinalPrice = o.Price - (_context.Discounts.Where(d => d.DiscountId == o.DiscountId).Select(d => Math.Round(o.Price * (d.Rate / 100m), 2)).FirstOrDefault()), // คำนวณราคาสุทธิ (อาจจะซับซ้อน)
                        Status = "เสร็จสิ้น" // TODO: เพิ่ม Logic กำหนดสถานะจริงๆ (เช่น ดูจากวันที่ หรือมี Field Status ใน Order)
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return Json(bookings); // ส่ง List กลับไปเป็น JSON
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user bookings for UserId {UserId}", currentUserId);
                return StatusCode(500, new { message = "เกิดข้อผิดพลาดในการดึงข้อมูลประวัติ" });
            }
        }

        [HttpPost] // ใช้ POST สำหรับการลบ
        [Authorize] // ต้อง Login
        [ValidateAntiForgeryToken] // *** สำคัญ: ต้องส่ง Token มากับ AJAX ***
        public async Task<IActionResult> DeleteBooking([FromBody] DeleteBookingRequest request) // รับ OrderId จาก JSON Body
        {
            if (request == null || request.OrderId <= 0)
            {
                return BadRequest(new { success = false, message = "OrderId ไม่ถูกต้อง" });
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null) { return Unauthorized(new { success = false, message = "ไม่พบผู้ใช้" }); }
            int currentUserId = currentUser.UserId;

            _logger.LogInformation("Attempting to delete OrderId {OrderId} for UserId {UserId}", request.OrderId, currentUserId);

            try
            {
                // *** ห้ามใช้ AsNoTracking() เพราะจะลบ ***
                var orderToDelete = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.UserId == currentUserId); // <-- เช็ค UserId

                if (orderToDelete == null)
                {
                    _logger.LogWarning("Delete failed: Order {OrderId} not found or access denied for UserId {UserId}", request.OrderId, currentUserId);
                    return NotFound(new { success = false, message = "ไม่พบรายการจองที่จะลบ หรือคุณไม่มีสิทธิ์" });
                }

                // --- ทำการลบ ---
                _context.Orders.Remove(orderToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted OrderId {OrderId}", request.OrderId);
                return Ok(new { success = true, message = $"ลบรายการจอง #{request.OrderId} เรียบร้อยแล้ว" });
            }
            catch (DbUpdateException ex) // อาจเกิด Error ถ้ามี FK Constraints
            {
                _logger.LogError(ex, "DbUpdateException deleting OrderId {OrderId}", request.OrderId);
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดฐานข้อมูลขณะลบ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting OrderId {OrderId}", request.OrderId);
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดขณะลบรายการจอง" });
            }
        }

        // (เพิ่ม) Class สำหรับรับค่า OrderId จาก JSON Body
        public class DeleteBookingRequest
        {
            public int OrderId { get; set; }
        }
    }
}