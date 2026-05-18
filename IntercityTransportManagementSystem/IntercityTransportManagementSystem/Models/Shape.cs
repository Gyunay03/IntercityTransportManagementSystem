using NetTopologySuite.Geometries;

namespace IntercityTransportManagementSystem.Models
{
    public class Shape
    {
        public int ShapeId { get; set; }
        public int Sequence { get; set; }
        public Point Location { get; set; } = null!;
    }
}
