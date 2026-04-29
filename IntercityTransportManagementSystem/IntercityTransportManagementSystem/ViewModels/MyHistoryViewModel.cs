using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class MyHistoryViewModel
    {
        public List<Reservation> ActiveOrPastReservations { get; set; }
        public List<Payment> PastPayments { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}
