using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Enums
{
    public enum UserRole
    {
        [Display(Name = "Администратор")]
        Administrator = 1,
        
        [Display(Name = "Пътник")]
        Passenger = 2,
        
        [Display(Name = "Шофьор")]
        Driver = 3
    }
}
