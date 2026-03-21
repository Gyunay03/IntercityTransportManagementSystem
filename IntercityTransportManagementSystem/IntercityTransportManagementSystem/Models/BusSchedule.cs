using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Models;

public partial class BusSchedule
{
    public int Id { get; set; }

    [Display(Name = "Маршрут")]
    [Required(ErrorMessage = "Моля, изберете маршрут.")]
    public int RouteId { get; set; }

    [Display(Name = "Автобус")]
    [Required(ErrorMessage = "Моля, изберете автобус.")]
    public int BusId { get; set; }

    [Display(Name = "Шофьор")]
    [Required(ErrorMessage = "Моля, изберете шофьор.")]
    public int DriverId { get; set; }

    [Display(Name = "Дата на пътуване")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "Моля, въведете/изберете дата на пътуване.")]
    public DateOnly TravelDate { get; set; }

    [Display(Name = "Час на тръгване")]
    [DataType(DataType.Time)]
    [Required(ErrorMessage = "Моля, въведете/изберете час на тръгване.")]
    public TimeOnly DepartureTime { get; set; }

    [Display(Name = "Час на пристигане")]
    [DataType(DataType.Time)]
    [Required(ErrorMessage = "Моля, въведете/изберете час на пристигане.")]
    public TimeOnly ArrivalTime { get; set; }

    [ValidateNever]
    public virtual Bus Bus { get; set; } = null!;

    [ValidateNever]
    public virtual Driver Driver { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [ValidateNever]
    public virtual Route Route { get; set; } = null!;
}
