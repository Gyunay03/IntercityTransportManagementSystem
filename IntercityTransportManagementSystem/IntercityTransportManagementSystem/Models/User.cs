using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IntercityTransportManagementSystem.Enums;

namespace IntercityTransportManagementSystem.Models;

public partial class User
{
    [Display(Name = "ИД")]
    public int Id { get; set; }
    [Required(ErrorMessage = "Моля, въведете име.")]
    [Display(Name = "Име")]
    public string Name { get; set; } = null!;
    [Required(ErrorMessage = "Моля, въведете фамилия.")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = null!;
    [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
    [Display(Name = "Имейл адрес")]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "Моля, въведете парола.")]
    [Display(Name = "Парола")]
    public string Password { get; set; } = null!;
    [Display(Name = "Роля")]
    public UserRole Role { get; set; }
    [Display(Name = "Статус")]
    public bool IsActive { get; set; }
    [Display(Name = "Дата на създаване")]
    public DateTime CreatedAt { get; set; }
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiration { get; set; }
    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationTokenExpiration { get; set; }
    [Display(Name = "Имейл")]
    public bool IsEmailVerified { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
}
