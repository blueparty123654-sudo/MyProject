// ใน Controllers/RedeemController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data; // <<< ตรวจสอบ Namespace
using MyProject.Models; // <<< ตรวจสอบ Namespace
using MyProject.ViewModels; // <<< ตรวจสอบ Namespace
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyProject.Controllers // <<< ตรวจสอบ Namespace
{
    [Authorize] // ต้อง Login เพื่อเข้าหน้านี้
    public class RedeemController : Controller
    {
        private readonly MyBookstoreDbContext _context; // <<< ตรวจสอบชื่อ DbContext
        private readonly ILogger<RedeemController> _logger;

        public RedeemController(MyBookstoreDbContext context, ILogger<RedeemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- GET: แสดงหน้ารายการของรางวัล ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. ดึงข้อมูล User ปัจจุบัน (เพื่อเอา Point) - *** ใช้ Tracking เผื่อต้อง Update ตอน Redeem ***
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail); // <<< ห้าม AsNoTracking()

            if (currentUser == null)
            {
                // ไม่ควรเกิดขึ้นถ้า [Authorize] ทำงาน แต่เช็คเผื่อไว้
                _logger.LogWarning("Redeem page accessed but user not found (Email: {UserEmail})", userEmail);
                return Challenge(); // Redirect ไป Login
            }
            int currentUserPoints = currentUser.UserPoint; // <<< อ่าน Point ปัจจุบัน

            // 2. ดึงรายการของรางวัลทั้งหมดจากตาราง Giveaways
            var giveaways = await _context.Giveaways
                                        .AsNoTracking() // ใช้ NoTracking ได้ เพราะแค่แสดงผล
                                        .OrderBy(g => g.PointCost) // เรียงตามคะแนนน้อยไปมาก (Optional)
                                        .Select(g => new GiveawayItemViewModel
                                        {
                                            GiveawayId = g.GiveawayId,
                                            Name = g.Name,
                                            ImageUrl = g.ImageUrl, // <<< ดึง ImageUrl
                                            PointCost = g.PointCost
                                        })
                                        .ToListAsync();

            // 3. สร้าง ViewModel สำหรับ View
            var viewModel = new RedeemViewModel
            {
                CurrentUserPoints = currentUserPoints,
                Giveaways = giveaways
            };

            _logger.LogInformation("Showing Redeem page for UserId {UserId} with {Points} points.", currentUser.UserId, currentUserPoints);

            // 4. ส่ง ViewModel ไปให้ View
            return View(viewModel);
        }

        // --- POST: (Placeholder) สำหรับ xử lý การกดแลก ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemItem(int giveawayId) // รับ ID ของรางวัล
        {
            // 1. ดึง User ปัจจุบัน (*** ต้องใช้ Tracking เพื่อ Update Point ***)
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail); // <<< ห้าม AsNoTracking()

            if (currentUser == null)
            {
                // กรณี Session หมดอายุ หรือหา User ไม่เจอ
                return Json(new { success = false, message = "ไม่พบข้อมูลผู้ใช้ กรุณาลองเข้าสู่ระบบใหม่" });
            }
            int currentUserId = currentUser.UserId;

            // 2. ดึงข้อมูลของรางวัล (ไม่ต้อง Tracking)
            var giveaway = await _context.Giveaways.AsNoTracking().FirstOrDefaultAsync(g => g.GiveawayId == giveawayId);

            if (giveaway == null)
            {
                _logger.LogWarning("Redeem failed: GiveawayId {GiveawayId} not found.", giveawayId);
                return Json(new { success = false, message = "ไม่พบของรางวัลที่ต้องการแลก" });
            }

            _logger.LogInformation("Attempting redemption for UserId {UserId}, GiveawayId {GiveawayId} ('{GiveawayName}') costing {PointCost} points. User has {UserPoints} points.",
                currentUserId, giveawayId, giveaway.Name, giveaway.PointCost, currentUser.UserPoint);

            // 3. เช็คคะแนนว่าเพียงพอหรือไม่
            if (currentUser.UserPoint < giveaway.PointCost)
            {
                _logger.LogWarning("Redeem failed: Insufficient points for UserId {UserId} attempting GiveawayId {GiveawayId}.", currentUserId, giveawayId);
                return Json(new { success = false, message = $"คะแนนไม่เพียงพอ ต้องการ {giveaway.PointCost} คะแนน แต่คุณมี {currentUser.UserPoint} คะแนน" });
            }

            // --- ถ้าคะแนนพอ ให้ดำเนินการแลก ---
            try
            {
                // 4. ลด Point ของ User
                currentUser.UserPoint -= giveaway.PointCost;

                // 5. สร้าง Record การแลก (Redemption)
                var newRedemption = new Redemption
                {
                    UserId = currentUserId,
                    GiveawayId = giveawayId,
                    RedemptionDate = DateTime.UtcNow, // ใช้ UTC ดีกว่า
                    Status = "Processing" // สถานะเริ่มต้น
                };
                _context.Redemptions.Add(newRedemption);

                // 6. บันทึกการเปลี่ยนแปลง (ทั้ง User Point และ Redemption ใหม่)
                await _context.SaveChangesAsync();

                _logger.LogInformation("Redemption successful for UserId {UserId}, GiveawayId {GiveawayId}. New point balance: {NewPoints}",
                    currentUserId, giveawayId, currentUser.UserPoint);

                // 7. ส่งผลลัพธ์ Success กลับไปให้ AJAX
                return Json(new
                {
                    success = true,
                    message = $"แลก '{giveaway.Name}' สำเร็จ! คะแนนคงเหลือ {currentUser.UserPoint} คะแนน",
                    newPoints = currentUser.UserPoint // ส่งคะแนนใหม่กลับไปด้วย
                });
            }
            catch (DbUpdateException ex)
            {
                // อาจเกิด Error ถ้าพยายามแลกซ้ำ (ถ้า Composite Key ป้องกัน) หรือ DB Error อื่นๆ
                _logger.LogError(ex, "DbUpdateException during redemption for UserId {UserId}, GiveawayId {GiveawayId}", currentUserId, giveawayId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการบันทึกข้อมูลการแลก" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during redemption for UserId {UserId}, GiveawayId {GiveawayId}", currentUserId, giveawayId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดไม่คาดคิดขณะดำเนินการแลก" });
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserRedemptions()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null) { return Unauthorized(new { message = "ไม่พบผู้ใช้" }); }
            int currentUserId = currentUser.UserId;

            _logger.LogInformation("Fetching redemption history for UserId {UserId}", currentUserId);

            try
            {
                var redemptions = await _context.Redemptions
                    .Where(r => r.UserId == currentUserId)
                    .Include(r => r.Giveaway) // <<< Join กับ Giveaway เพื่อเอาชื่อ/รูป/PointCost
                    .OrderByDescending(r => r.RedemptionDate) // เรียงตามล่าสุดก่อน
                    .Select(r => new RedemptionHistoryItemViewModel
                    {
                        RedemptionId = r.RedemptionId, // <<< เพิ่ม ID การแลก
                        GiveawayName = r.Giveaway.Name,
                        GiveawayImageUrl = r.Giveaway.ImageUrl,
                        RedemptionDate = r.RedemptionDate,
                        Status = r.Status,
                        PointCost = r.Giveaway.PointCost // <<< เพิ่ม PointCost
                    })
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Found {Count} redemption records for UserId {UserId}", redemptions.Count, currentUserId);
                return Json(redemptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching redemption history for UserId {UserId}", currentUserId);
                return StatusCode(500, new { message = "เกิดข้อผิดพลาดในการดึงข้อมูลประวัติการแลกคะแนน" });
            }
        }
    }
}