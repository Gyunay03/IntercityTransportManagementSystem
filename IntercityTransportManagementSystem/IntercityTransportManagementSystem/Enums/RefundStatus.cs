using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Enums
{
    public enum RefundStatus
    {
        [Display(Name = "Чакащо")]
        Pending = 1,
        
        [Display(Name = "Завършено")]
        Completed = 2,
        
        [Display(Name = "Отказано")]
        Rejected = 3
    }
}
