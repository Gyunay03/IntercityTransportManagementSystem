using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Enums
{
    public enum PaymentMethod : byte
    {
        [Display(Name = "В брой (на каса/при шофьор)")]
        Cash = 1,
        
        [Display(Name = "С карта")]
        Card = 2,
        
        [Display(Name = "Онлайн")]
        Online = 3
    }
}
