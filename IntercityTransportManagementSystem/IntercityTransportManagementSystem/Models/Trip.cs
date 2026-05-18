using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntercityTransportManagementSystem.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }
        public string? TripOriginalId { get; set; }
        public int LineId { get; set; }
        [ForeignKey("LineId")]
        public TransportLine TransportLine { get; set; } = null!;
        public int? ShapeId { get; set; }
        public bool? DirectionId { get; set; }

        public ICollection<StopTime> StopTimes { get; set; } = new List<StopTime>();
    }
}
