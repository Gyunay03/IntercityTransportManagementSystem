using IntercityTransportManagementSystem.Enums;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Моля, въведете име.")]
        [Display(Name = "Име")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Моля, въведете фамилия.")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
        [Display(Name = "Имейл адрес")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Роля")]
        public UserRole Role { get; set; }

        [Display(Name = "Статус")]
        public bool IsActive { get; set; }
        
        [Display(Name = "Имейл")]
        public bool IsEmailVerified { get; set; }
    }
}