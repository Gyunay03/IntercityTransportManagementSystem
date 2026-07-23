using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Enums
{
    public enum NotificationType
    {
        [Display(Name = "Купен билет")]
        TicketPurchased = 1,
        
        [Display(Name = "Създадена резервация")]
        ReservationCreated = 2,
        
        [Display(Name = "Закъснял автобус")]
        BusDelayed = 3,
        
        [Display(Name = "Променено разписание")]
        ScheduleChanged = 4,
        
        [Display(Name = "Напомняне за пътуване")]
        TripReminder = 5,
        
        [Display(Name = "Системно известие")]
        SystemMessage = 6
    }
}
