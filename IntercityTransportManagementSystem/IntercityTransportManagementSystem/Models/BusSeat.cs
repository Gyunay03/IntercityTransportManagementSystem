using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Models;

public partial class BusSeat
{
    public int Id { get; set; }

    [Display(Name = "Номер")]
    [Required(ErrorMessage = "Моля, изберете/въведете номер в автобуса.")]
    public int Number { get; set; }
    [Display(Name = "Автобус")]
    public int BusId { get; set; }
    [Display(Name = "Автобус")]
    public virtual Bus Bus { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
