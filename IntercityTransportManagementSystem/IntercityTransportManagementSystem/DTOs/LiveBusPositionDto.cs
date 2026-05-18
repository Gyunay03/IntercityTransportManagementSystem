namespace IntercityTransportManagementSystem.DTOs
{
    public class LiveBusPositionDto
    {
        public int TripId { get; set; }
        public string VehicleNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsRealTime { get; set; }
        public double Speed { get; set; }
        public double? Heading { get; set; }
    }
}
