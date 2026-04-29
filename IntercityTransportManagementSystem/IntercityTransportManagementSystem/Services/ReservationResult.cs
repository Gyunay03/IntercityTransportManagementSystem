using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.Services
{
    public class ReservationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ReservationId { get; set; }
        public Reservation Reservation { get; set; }
    }
}
