// ใน ViewModels/AdminRedemptionViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // เพิ่ม using นี้

namespace MyProject.ViewModels
{
    public class AdminRedemptionViewModel
    {
        public int RedemptionId { get; set; } // ID ของการแลก (เผื่อใช้)
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty; // ชื่อผู้ใช้
        public int GiveawayId { get; set; }
        public string GiveawayName { get; set; } = string.Empty; // ชื่อของรางวัล
        public DateTime RedemptionDate { get; set; }
        public string CurrentStatus { get; set; } = string.Empty; // สถานะปัจจุบัน

        // สำหรับ Dropdown หรือ Input เพื่อเปลี่ยน Status
        [Required(ErrorMessage = "กรุณาเลือกสถานะใหม่")]
        public string NewStatus { get; set; } = string.Empty;

        // ตัวเลือก Status ที่เป็นไปได้ (สำหรับ Dropdown)
        public static List<string> AvailableStatuses { get; } = new List<string> { "Processing", "Shipped", "Completed", "Cancelled" }; // เพิ่ม/แก้ไขตามต้องการ
    }
}