// ใน ViewModels/GiveawayItemViewModel.cs
namespace MyProject.ViewModels
{
    public class GiveawayItemViewModel
    {
        public int GiveawayId { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = string.Empty;
        public int PointCost { get; set; }
    }
}