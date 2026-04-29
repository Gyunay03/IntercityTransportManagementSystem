using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Чакащо")]
        Pending = 1,
        
        [Display(Name = "Завършено")]
        Completed = 2,
        
        [Display(Name = "Анулирано")]
        Cancelled = 3,
        
        [Display(Name = "Върнато")]
        Refunded = 4
    }
}
