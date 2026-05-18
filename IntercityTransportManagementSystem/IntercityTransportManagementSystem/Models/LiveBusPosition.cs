using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntercityTransportManagementSystem.Models
{
    public class LiveBusPosition
    {
        [Key]
        public int BusId { get; set; }

        public int TripId { get; set; }
        [ForeignKey("TripId")]
        public Trip Trip { get; set; } = null!;

        public Point CurrentLocation { get; set; } = null!;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public bool IsRealTime { get; set; }
        public string? VehicleNumber { get; set; }
        public double? Heading { get; set; }
        public double? Speed { get; set; }
    }
}
