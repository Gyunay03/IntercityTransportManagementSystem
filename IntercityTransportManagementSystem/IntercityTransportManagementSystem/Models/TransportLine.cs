using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Models
{
    public class TransportLine
    {
        [Key]
        public int LineId { get; set; }
        public string? LineOriginalId { get; set; }
        public string? ShortName { get; set; }
        public string? LongName { get; set; }
        public int RouteType { get; set; }
        
        public int? BusinessRouteId { get; set; }

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
