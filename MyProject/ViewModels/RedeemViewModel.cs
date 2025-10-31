// ใน ViewModels/RedeemViewModel.cs
using System.Collections.Generic;

namespace MyProject.ViewModels
{
    public class RedeemViewModel
    {
        public int CurrentUserPoints { get; set; } // คะแนนปัจจุบันของผู้ใช้
        public List<GiveawayItemViewModel> Giveaways { get; set; } = new List<GiveawayItemViewModel>(); // รายการของรางวัล
    }
}