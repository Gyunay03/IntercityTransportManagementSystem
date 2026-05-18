using System.ComponentModel.DataAnnotations.Schema;

namespace IntercityTransportManagementSystem.Models
{
    public class StopTime
    {
        public int TripId { get; set; }
        [ForeignKey("TripId")]
        public Trip Trip { get; set; } = null!;

        public int StopId { get; set; }
        [ForeignKey("StopId")]
        public Stop Stop { get; set; } = null!;

        public TimeSpan ArrivalTime { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public int StopSequence { get; set; }
    }
}
