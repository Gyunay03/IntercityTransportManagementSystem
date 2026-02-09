using System;
using System.Collections.Generic;

namespace IntercityTransportManagementSystem.Models;

public partial class Reservation
{
    public int Id { get; set; }

    public int PassengerId { get; set; }

    public int ScheduleId { get; set; }

    public int SeatId { get; set; }

    public byte Status { get; set; }

    public DateTime ReservationTime { get; set; }

    public virtual Passenger Passenger { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual BusSchedule Schedule { get; set; } = null!;

    public virtual BusSeat Seat { get; set; } = null!;
}
