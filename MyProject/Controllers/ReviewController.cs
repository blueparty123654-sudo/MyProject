using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;    
using MyProject.Data;                  
using MyProject.Models;                 
using MyProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyProject.Controllers
{
    public class ReviewController : Controller // 👈 ชื่อคลาส
    {
        // (เพิ่ม) Dependencies
        private readonly MyBookstoreDbContext _context;
        // private readonly ILogger<ReviewController> _logger; // อาจจะเพิ่ม Logger ด้วยก็ได้

        // (เพิ่ม) Constructor
        public ReviewController(MyBookstoreDbContext context /*, ILogger<ReviewController> logger */)
        {
            _context = context;
            // _logger = logger;
        }

        // --- Action Reviews ---
        // (วางโค้ด Reviews() ที่ตัดมา)
        public async Task<IActionResult> Reviews()
        {
            var viewModel = new ReviewPageViewModel();

            // 1. ดึงข้อมูลสำหรับ Dropdown (เหมือนเดิม)
            viewModel.Products = await _context.Products.OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.ProductId.ToString(), Text = p.Name }).ToListAsync();
            viewModel.Branches = await _context.Branches.OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.BranchId.ToString(), Text = b.Name }).ToListAsync();

            // 2. ดึงข้อมูลผู้ใช้ปัจจุบัน (เหมือนเดิม)
            var currentUserId = 0;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
                if (currentUser != null) { currentUserId = currentUser.UserId; }
            }

            // 3. (แก้ไข) นับจำนวนรีวิวทั้งหมด
            viewModel.TotalReviewCount = await _context.Reviews.CountAsync();

            // 4. (แก้ไข) ดึงข้อมูลดิบมาแค่ 5 อันแรก
            var rawReviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.Branch)
                .OrderByDescending(r => r.ReviewDate)
                .Take(3)
                .Select(r => new
                {
                    ReviewId = r.ReviewId,
                    AuthorUserId = r.UserId,
                    UserName = r.User.Name,
                    ProductName = r.Product != null ? r.Product.Name : null,
                    BranchName = r.Branch != null ? r.Branch.Name : null,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate
                })
                .ToListAsync();

            // 5. แปลงข้อมูลดิบ (เหมือนเดิม)
            viewModel.Reviews = rawReviews.Select(r => new ReviewItemViewModel
            {
                ReviewId = r.ReviewId,
                IsOwner = (r.AuthorUserId == currentUserId || User.IsInRole("Admin")),
                UserName = r.UserName ?? "Anonymous",
                ProductName = r.ProductName,
                BranchName = r.BranchName,
                Rating = r.Rating,
                Comment = r.Comment,
                PostedAgo = TimeAgo(r.ReviewDate)
            }).ToList();

            // 6. (เพิ่ม) บันทึกว่าเราแสดงผลไปแล้วกี่อัน
            viewModel.ReviewsCurrentlyDisplayed = viewModel.Reviews.Count;

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


        [HttpPost]                             
        [Authorize]                            // อนุญาตเฉพาะคนที่ Login แล้ว
        [ValidateAntiForgeryToken]             // 👈 ป้องกันการโจมตีแบบ CSRF
        public async Task<IActionResult> SubmitReview(ReviewInputViewModel model)
        {
            // 1. ตรวจสอบ Validation เบื้องต้น (Required, Range, StringLength)
            if (!ModelState.IsValid)
            {
                // ส่งรายการ Error กลับไปให้ AJAX (เหมือนตอน Register/UpdateProfile)
                return Json(new { success = false, errors = ModelStateToDictionary() });
            }

            // 2. ดึง UserId ของผู้ใช้ที่ Login อยู่ปัจจุบัน
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.AsNoTracking()
                                            .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (currentUser == null)
            {
                // กรณีนี้ไม่ควรเกิดถ้า [Authorize] ทำงานถูกต้อง แต่เป็นการป้องกันไว้ก่อน
                return Json(new { success = false, message = "เกิดข้อผิดพลาด: ไม่พบข้อมูลผู้ใช้" });
            }

            // 3. สร้าง Object Review ใหม่
            var review = new Review
            {
                UserId = currentUser.UserId, // 👈 ใช้ UserId (int) ที่ดึงมา
                ProductId = model.ProductId,
                BranchId = model.BranchId,
                Rating = model.Rating,
                Comment = model.Comment,
                ReviewDate = DateTime.Now // 👈 บันทึกเวลาปัจจุบัน
            };

            // 4. บันทึกลงฐานข้อมูล
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // 5. ส่งผลลัพธ์ว่าสำเร็จกลับไป
            // เราอาจจะส่งข้อมูลรีวิวที่เพิ่งสร้างกลับไปด้วย เพื่อให้ JS เอาไปแสดงผลทันที
            return Json(new
            {
                success = true,
                message = "ส่งรีวิวของคุณเรียบร้อยแล้ว!",
                newReview = new ReviewItemViewModel // 👈 ส่งข้อมูลรีวิวใหม่กลับไปด้วย
                {
                    UserName = currentUser.Name ?? "Anonymous",
                    ProductName = model.ProductId.HasValue ? (await _context.Products.FindAsync(model.ProductId.Value))?.Name : null,
                    BranchName = model.BranchId.HasValue ? (await _context.Branches.FindAsync(model.BranchId.Value))?.Name : null,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    PostedAgo = "เมื่อสักครู่" // แสดงผลแบบง่ายๆ ก่อน
                }
            });
        }

        // (อย่าลืม) ตรวจสอบว่ามีฟังก์ชัน ModelStateToDictionary() อยู่ใน Controller นี้ด้วย
        private Dictionary<string, string[]> ModelStateToDictionary()
        {
            return ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
            );
        }


        [HttpPost]
        [Authorize] // 👈 บังคับให้ต้อง Login ก่อน
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            // 1. ดึงข้อมูลผู้ใช้ปัจจุบัน
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "ไม่พบผู้ใช้" });
            }

            // 2. ค้นหารีวิว
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "ไม่พบรีวิวที่ต้องการลบ" });
            }

            // 3. (สำคัญ!) ตรวจสอบสิทธิ์อีกครั้งที่ฝั่งเซิร์ฟเวอร์
            // ต้องเป็น "เจ้าของ" รีวิว หรือ เป็น "Admin" เท่านั้น
            if (review.UserId != currentUser.UserId && !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "คุณไม่มีสิทธิ์ลบรีวิวนี้" });
            }

            // 4. ถ้าสิทธิ์ถูกต้อง -> ลบ
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "ลบรีวิวเรียบร้อยแล้ว" });
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreReviews(int skip)
        {
            int take = 3; // โหลดทีละ 5 อัน

            // ดึงข้อมูลผู้ใช้ปัจจุบัน (เพื่อเช็ค IsOwner)
            var currentUserId = 0;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
                if (currentUser != null) { currentUserId = currentUser.UserId; }
            }

            // 1. ดึงข้อมูลดิบชุดถัดไป
            var rawReviews = await _context.Reviews
                .Include(r => r.User).Include(r => r.Product).Include(r => r.Branch)
                .OrderByDescending(r => r.ReviewDate)
                .Skip(skip) // 👈 ข้ามอันที่แสดงไปแล้ว
                .Take(take) // 👈 ดึงมา 5 อันใหม่
                .Select(r => new {
                    ReviewId = r.ReviewId,
                    AuthorUserId = r.UserId,
                    UserName = r.User.Name,
                    ProductName = r.Product != null ? r.Product.Name : null,
                    BranchName = r.Branch != null ? r.Branch.Name : null,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate
                }).ToListAsync();

            // 2. แปลงข้อมูลดิบ
            var newReviews = rawReviews.Select(r => new ReviewItemViewModel
            {
                ReviewId = r.ReviewId,
                IsOwner = (r.AuthorUserId == currentUserId || User.IsInRole("Admin")),
                UserName = r.UserName ?? "Anonymous",
                ProductName = r.ProductName,
                BranchName = r.BranchName,
                Rating = r.Rating,
                Comment = r.Comment,
                PostedAgo = TimeAgo(r.ReviewDate)
            }).ToList();

            // 3. นับจำนวนรีวิวทั้งหมดที่เหลือ
            var totalCount = await _context.Reviews.CountAsync();
            bool hasMore = (skip + newReviews.Count) < totalCount;

            // 4. ส่ง JSON กลับไป
            return Json(new { reviews = newReviews, hasMore = hasMore });
        }
    }
}