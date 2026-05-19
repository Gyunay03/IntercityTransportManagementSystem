using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Hubs;

namespace IntercityTransportManagementSystem.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;
        private readonly IHubContext<ReservationHub> _hub;
        private readonly INotificationService _notificationService;

        public ReservationService(IntercityTransportManagementSystemDatabaseContext context, IHubContext<ReservationHub> hub, INotificationService notificationService)
        {
            _context = context;
            _hub = hub;
            _notificationService = notificationService;
        }

        public async Task<ReservationResult> ConfirmSeatAsync(int scheduleId, int seatId, int passengerId, TicketType ticketType, int? outboundReservationId = null)
        {
            var now = DateTime.Now;

            if (await IsSeatTaken(scheduleId, seatId, passengerId))
            {
                return new ReservationResult { Success = false, Message = "Мястото вече е заето или заключено." };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingLock = await _context.BusSeatLocks
                    .FirstOrDefaultAsync(l => l.ScheduleId == scheduleId && l.SeatId == seatId);

                if (existingLock != null)
                {
                    _context.BusSeatLocks.Remove(existingLock);
                }

                var newReservation = new Reservation
                {
                    ScheduleId = scheduleId,
                    SeatId = seatId,
                    PassengerId = passengerId,
                    ReservationTime = now,
                    Status = ReservationStatus.Pending,
                    IsActive = true,
                    ExpirationTime = now.AddMinutes(60),
                    TicketType = ticketType,
                    ReturnReservationId = outboundReservationId
                };

                _context.Add(newReservation);
                await _context.SaveChangesAsync();

                if (outboundReservationId.HasValue)
                {
                    var outbound = await _context.Reservations.FindAsync(outboundReservationId.Value);
                    if (outbound != null)
                    {
                        outbound.ReturnReservationId = newReservation.Id;
                        _context.Update(outbound);
                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                var reservationInfo = await _context.Reservations
                    .Include(r => r.Passenger)
                    .Include(r => r.Seat)
                    .Include(r => r.Schedule)
                        .ThenInclude(s => s.Route)
                    .FirstOrDefaultAsync(r => r.Id == newReservation.Id);

                if (reservationInfo?.Passenger.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(reservationInfo.Passenger.UserId.Value, "Успешна резервация",
                        $"Място {reservationInfo.Seat.Number} е резервирано за пътуване " +
                        $"{reservationInfo.Schedule.Route.StartDestination} - {reservationInfo.Schedule.Route.FinalDestination} " +
                        $"на {reservationInfo.Schedule.TravelDate:dd.MM.yyyy} в {reservationInfo.Schedule.DepartureTime:HH:mm} часа.",
                        NotificationType.ReservationCreated);
                }

                await _hub.Clients.All.SendAsync("SeatReserved", new { scheduleId, seatId });

                return new ReservationResult { Success = true, Reservation = newReservation, ReservationId = newReservation.Id };
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                return new ReservationResult { Success = false, Message = "Възникна грешка при запазването на резервацията." };
            }
        }

        private async Task<bool> IsSeatTaken(int scheduleId, int seatId, int passengerId)
        {
            var now = DateTime.Now;
            var isReserved = await _context.Reservations.AnyAsync(r =>
                r.ScheduleId == scheduleId && r.SeatId == seatId && r.IsActive &&
                (r.Status == ReservationStatus.Confirmed || (r.Status == ReservationStatus.Pending && r.ExpirationTime > now)));

            var isLocked = await _context.BusSeatLocks.AnyAsync(l =>
                l.ScheduleId == scheduleId && l.SeatId == seatId && l.ExpiryTime > now && l.PassengerId != passengerId);

            return isReserved || isLocked;
        }
    }
}
