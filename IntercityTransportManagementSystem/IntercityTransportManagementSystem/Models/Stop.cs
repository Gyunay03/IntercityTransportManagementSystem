using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Models
{
    public class Stop
    {
        [Key]
        public int StopId { get; set; }
        public string? StopOriginalId { get; set; }
        [Required]
        public string StopName { get; set; } = null!;
        public string? StopCode { get; set; }

        public Point Location { get; set; } = null!;
    }
}
