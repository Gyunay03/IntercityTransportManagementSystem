using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Enums
{
    public enum ReservationStatus : byte
    {
        [Display(Name = "Чакаща")]
        Pending = 1,
        
        [Display(Name = "Потвърдена")]
        Confirmed = 2,
        
        [Display(Name = "Отменена")]
        Cancelled = 3,
        
        [Display(Name = "Изтекла")]
        Expired = 4
    }
}
