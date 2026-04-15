using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Models;

public partial class Passenger
{
    public int Id { get; set; }

    [Display(Name = "Име")]
    public string Name { get; set; } = null!;

    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Имейл адрес")]
    public string Email { get; set; } = null!;

    public int? UserId { get; set; }

    public User? User { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
