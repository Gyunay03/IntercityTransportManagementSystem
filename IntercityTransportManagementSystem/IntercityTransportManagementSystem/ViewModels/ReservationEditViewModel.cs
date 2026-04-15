using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class ReservationEditViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Пътник")]
        public int PassengerId { get; set; }

        [Display(Name = "Маршрут")]
        public int RouteId { get; set; }
        
        [Display(Name = "Разписание")]
        public int ScheduleId { get; set; }
        
        [Display(Name = "Място")]
        public int SeatId { get; set; }

        [Display(Name = "Статус")]
        public ReservationStatus Status { get; set; }

        [Display(Name = "Дата на резервация")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime ReservationTime { get; set; }

        public List<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
        public List<BusSeat> BusSeats { get; set; } = new List<BusSeat>();
    }
}
