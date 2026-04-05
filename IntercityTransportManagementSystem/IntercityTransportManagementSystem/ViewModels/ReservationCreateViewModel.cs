using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class ReservationCreateViewModel
    {
        public int ScheduleId { get; set; }
        public int SeatId { get; set; }

        public IEnumerable<BusSchedule> Schedules { get; set; }
        public IEnumerable<BusSeat> Seats { get; set; }
    }
}
