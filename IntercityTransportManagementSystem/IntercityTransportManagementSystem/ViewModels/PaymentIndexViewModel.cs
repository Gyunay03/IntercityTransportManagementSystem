using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class PaymentIndexViewModel
    {
        public IEnumerable<Payment> Payments { get; set; }
        public string SearchString { get; set; }
        public string SortOrder { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
