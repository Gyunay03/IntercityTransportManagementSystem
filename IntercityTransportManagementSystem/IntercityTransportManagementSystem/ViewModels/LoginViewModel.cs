using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
        [EmailAddress(ErrorMessage = "Въвели сте невалиден имейл адрес.")]
        [Display(Name = "Имейл адрес")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Моля, въведете парола.")]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; }

        [Display(Name = "Запомни ме")]
        public bool RememberMe { get; set; }
    }
}
