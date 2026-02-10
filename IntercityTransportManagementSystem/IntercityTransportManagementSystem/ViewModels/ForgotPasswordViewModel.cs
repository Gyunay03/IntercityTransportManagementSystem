using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
        [EmailAddress(ErrorMessage = "Въвели сте невалиден имейл адрес.")]
        [Display(Name = "Имейл адрес")]
        public string Email { get; set; }
    }
}
