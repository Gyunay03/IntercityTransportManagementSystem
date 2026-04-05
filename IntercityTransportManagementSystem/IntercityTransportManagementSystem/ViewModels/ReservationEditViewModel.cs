using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class ReservationEditViewModel
    {
        public int Id { get; set; }
        public int PassengerId { get; set; }
        public int ScheduleId { get; set; }
        public int SeatId { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime ReservationTime { get; set; }

        public List<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
        public List<BusSeat> BusSeats { get; set; } = new List<BusSeat>();
    }
}
