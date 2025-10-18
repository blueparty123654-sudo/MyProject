using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyProject.ViewModels
{
    // ViewModel สำหรับแสดงผลรีวิวแต่ละอัน
    public class ReviewItemViewModel
    {
        public int ReviewId { get; set; }
        public bool IsOwner { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public string? BranchName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string PostedAgo { get; set; } = string.Empty; // เช่น "5 ชั่วโมงที่แล้ว"
    }

    // ViewModel สำหรับทั้งหน้า
    public class ReviewPageViewModel
    {
        public List<SelectListItem> Products { get; set; } = new();
        public List<SelectListItem> Branches { get; set; } = new();
        public List<ReviewItemViewModel> Reviews { get; set; } = new();
        public int TotalReviewCount { get; set; }
        public int ReviewsCurrentlyDisplayed { get; set; }
    }
}