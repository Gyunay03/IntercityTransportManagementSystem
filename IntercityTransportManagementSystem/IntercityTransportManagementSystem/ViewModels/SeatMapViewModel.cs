using IntercityTransportManagementSystem.Enums;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class SeatMapViewModel
    {
        public int ScheduleId { get; set; }
        public int PassengerId { get; set; }
        public string BusRegistrationNumber { get; set; }
        public string RouteName { get; set; }
        public DateOnly TravelDate { get; set; }
        public TicketType TicketType { get; set; }
        public int? OutboundReservationId { get; set; }

        public List<SeatDto> Seats { get; set; }
    }

    public class SeatDto
    {
        public int SeatId { get; set; }
        public int Number { get; set; }
        public bool IsTaken { get; set; }
        public bool IsLocked { get; set; }
        public bool IsSelected { get; set; }
    }
}
