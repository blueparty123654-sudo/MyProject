// ใน Services/IBookingService.cs
using MyProject.Models;
using MyProject.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyProject.Services
{
    public class BookingCreationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? CreatedOrderId { get; set; }
    }
    public class BookingDeletionResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IBookingService
    {
        decimal CalculateOptimizedPrice(int numberOfDays, Product product);
        Task<BookingCreationResult> CreateBookingAsync(BookingInputViewModel model, string userEmail);
        Task<BookingDeletionResult> DeleteBookingAsync(int orderId, string userEmail);
        Task<List<RentalHistoryItemViewModel>> GetRentalHistoryAsync(string userEmail);
    }
}