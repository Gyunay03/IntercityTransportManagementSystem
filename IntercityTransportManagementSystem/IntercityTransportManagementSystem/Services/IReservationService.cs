using System.Threading.Tasks;
using IntercityTransportManagementSystem.Enums;

namespace IntercityTransportManagementSystem.Services
{
    public interface IReservationService
    {
        Task<ReservationResult> ConfirmSeatAsync(int scheduleId, int seatId, int passengerId, TicketType ticketType, int? outboundReservationId = null);
    }
}
