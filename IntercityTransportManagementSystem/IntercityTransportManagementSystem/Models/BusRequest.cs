using IntercityTransportManagementSystem.Enums;

namespace IntercityTransportManagementSystem.Models
{
    public partial class BusRequest
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public BusSchedule Schedule { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public BusRequestStatus Status { get; set; } = BusRequestStatus.Pending;
    }
}
