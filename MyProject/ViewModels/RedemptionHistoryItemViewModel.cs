// ใน ViewModels/RedemptionHistoryItemViewModel.cs
using System;

namespace MyProject.ViewModels
{
    public class RedemptionHistoryItemViewModel
    {
        public int RedemptionId { get; set; } // ID ของการแลก (เผื่อใช้)
        public string GiveawayName { get; set; } = null!;
        public string GiveawayImageUrl { get; set; } = string.Empty;
        public DateTime RedemptionDate { get; set; }
        public string Status { get; set; } = null!;
        public int PointCost { get; set; } // เพิ่ม Point ที่ใช้แลก
    }
}