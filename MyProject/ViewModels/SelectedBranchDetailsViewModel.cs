namespace MyProject.ViewModels
{
    public class SelectedBranchDetailsViewModel
    {
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MapUrl { get; set; }
    }
}