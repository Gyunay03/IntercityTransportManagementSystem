using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class ReservationIndexViewModel
    {
        public IEnumerable<Reservation> Reservations { get; set; }
        public string SearchString { get; set; }
        public string SortOrder { get; set; }
        public DateTime? ReservationTimeFrom { get; set; }
        public DateTime? ReservationTimeTo { get; set; }
        public DateOnly? TravelDate { get; set; }
        public ReservationStatus? Status { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
