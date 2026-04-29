using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class ReservationCreateViewModel
    {
        [Display(Name = "Пътник")]
        public int PassengerId { get; set; }

        [Display(Name = "Маршрут")]
        public int RouteId { get; set; }

        [Display(Name = "Разписание")]
        public int ScheduleId { get; set; }

        [Display(Name = "Място")]
        public int? SeatId { get; set; }

        public TicketType TicketType { get; set; }
        
        public IEnumerable<BusSchedule> ?Schedules { get; set; }
        public IEnumerable<BusSeat>? Seats { get; set; }
    }
}
