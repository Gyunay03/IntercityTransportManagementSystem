namespace IntercityTransportManagementSystem.DTOs
{
    public class TripRouteStopDto
    {
        public int StopSequence { get; set; }
        public string StopName { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
