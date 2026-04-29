using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Enums
{
    public enum TicketType
    {
        [Display(Name = "Еднопосочен")]
        Ednoposochen = 1,
        
        [Display(Name = "Двупосочен")]
        Dvuposochen = 2
    }
}
