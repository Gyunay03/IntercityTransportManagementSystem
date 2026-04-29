using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IntercityTransportManagementSystem.Enums;

namespace IntercityTransportManagementSystem.Models;

public partial class Payment
{
    public int Id { get; set; }

    [Display(Name = "Пътник")]
    public int PassengerId { get; set; }

    [Display(Name = "Резервация")]
    public int ReservationId { get; set; }

    [Display(Name = "Сума")]
    public decimal Sum { get; set; }

    [Display(Name = "Метод на плащане")]
    public PaymentMethod PaymentMethod { get; set; }

    [Display(Name = "Дата на плащане")]
    public DateTime PaymentDate { get; set; }

    [Display(Name = "Статус на плащане")]
    public PaymentStatus PaymentStatus { get; set; }
    
    public virtual Passenger Passenger { get; set; } = null!;

    public virtual Reservation Reservation { get; set; } = null!;
}
