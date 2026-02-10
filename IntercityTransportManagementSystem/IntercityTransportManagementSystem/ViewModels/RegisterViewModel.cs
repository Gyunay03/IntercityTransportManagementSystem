using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Моля, въведете име.")]
        [Display(Name = "Име")]
        public string FistName { get; set; }

        [Required(ErrorMessage = "Моля, въведете фамилия.")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
        [EmailAddress(ErrorMessage = "Въвели сте невалиден имейл адрес.")]
        [Display(Name = "Имейл адрес")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Моля, въведете парола.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Паролата трябва да е между {2} и {1} символа.")]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Моля, потвърдете паролата.")]
        [DataType(DataType.Password)]
        [Display(Name = "Потвърдете паролата.")]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; }
    }
}