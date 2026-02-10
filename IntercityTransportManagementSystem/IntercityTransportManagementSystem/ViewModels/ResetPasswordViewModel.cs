using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
        [EmailAddress(ErrorMessage = "Въвели сте невалиден имейл адрес.")]
        [Display(Name = "Имейл адрес")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Моля, въведете нова парола.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Паролата трябва да е между {2} и {1} символа.")]
        [DataType(DataType.Password)]
        [Display(Name = "Нова парола")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Моля, потвърдете паролата.")]
        [DataType(DataType.Password)]
        [Display(Name = "Потвърдете паролата.")]
        [Compare("NewPassword", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmThePassword { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
