using IntercityTransportManagementSystem.DTOs;

namespace IntercityTransportManagementSystem.Services
{
    public interface ITripService
    {
        List<TripRouteStopDto> GetRouteByTripId(int tripId);
        Task PopulateShapesForTrip(int tripId);
        List<object> GetFullPathByTripId(int tripId);
    }
}
