using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.Services
{
    public interface IVehicleProvider
    {
        string ProviderName { get; }
        Task<List<LiveBusPosition>> GetPositionsAsync();
    }
}
