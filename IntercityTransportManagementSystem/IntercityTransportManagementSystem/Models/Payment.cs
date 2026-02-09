using System;
using System.Collections.Generic;

namespace IntercityTransportManagementSystem.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int PassengerId { get; set; }

    public int ReservationId { get; set; }

    public decimal Sum { get; set; }

    public byte PaymentMethod { get; set; }

    public DateTime PaymentDate { get; set; }

    public virtual Passenger Passenger { get; set; } = null!;

    public virtual Reservation Reservation { get; set; } = null!;
}
