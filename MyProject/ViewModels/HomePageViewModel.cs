
namespace MyProject.ViewModels
{
    public class BranchFilterViewModel // ViewModel ย่อยสำหรับ Filter
    {
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class HomePageViewModel
    {
        public IEnumerable<ProductViewModel> Products { get; set; } = Enumerable.Empty<ProductViewModel>();
        public IEnumerable<BranchFilterViewModel> FilterBranches { get; set; } = Enumerable.Empty<BranchFilterViewModel>();
        public int? SelectedBranchId { get; set; }
        public string? SelectedFilterName { get; set; }
        public SelectedBranchDetailsViewModel? SelectedBranchDetails { get; set; }
    }
}