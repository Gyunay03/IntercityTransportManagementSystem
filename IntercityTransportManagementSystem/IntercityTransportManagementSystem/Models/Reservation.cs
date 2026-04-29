using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IntercityTransportManagementSystem.Enums;

namespace IntercityTransportManagementSystem.Models;

public partial class Reservation
{
    public int Id { get; set; }
    
    [Display(Name = "Пътник")]
    [Required(ErrorMessage = "Моля, изберете пътник.")]
    public int PassengerId { get; set; }

    [Display(Name = "Разписание")]
    [Required(ErrorMessage = "Моля, изберете разписание.")]
    public int ScheduleId { get; set; }

    [Display(Name = "Номер на място")]
    [Required(ErrorMessage = "Моля, изберете място в автобуса.")]
    public int SeatId { get; set; }

    [Display(Name = "Статус")]
    public ReservationStatus Status { get; set; }

    [Display(Name = "Дата на резервиране")]
    public DateTime ReservationTime { get; set; }

    [Display(Name = "Оставащо време на резервацията")]
    public DateTime? ExpirationTime { get; set; }

    [Display(Name = "Активност на резервация")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Заключване на място при избор")]
    public bool IsLocked { get; set; }

    [Display(Name = "Време на заключване на място при избор")]
    public DateTime? LockExpirationTime { get; set; }

    [Display(Name = "Тип на билета")]
    public TicketType TicketType { get; set; } 

    public int? ReturnReservationId { get; set; }
    public Reservation? ReturnReservation { get; set; }
    public virtual Passenger Passenger { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual BusSchedule Schedule { get; set; } = null!;

    public virtual BusSeat Seat { get; set; } = null!;
}
