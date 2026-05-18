using IntercityTransportManagementSystem.DTOs;

namespace IntercityTransportManagementSystem.Services
{
    public interface ILineService
    {
        List<LineDto> GetAllLines();
    }
}
