using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Enums
{
    public enum BusRequestStatus
    {
        [Display(Name = "Чакащ")]
        Pending = 1,
        
        [Display(Name = "Одобрен")]
        Approved = 2,
        
        [Display(Name = "Отказан")]
        Rejected = 3
    }
}
