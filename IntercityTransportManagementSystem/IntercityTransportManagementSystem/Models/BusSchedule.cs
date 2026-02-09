using System;
using System.Collections.Generic;

namespace IntercityTransportManagementSystem.Models;

public partial class BusSchedule
{
    public int Id { get; set; }

    public int RouteId { get; set; }

    public int BusId { get; set; }

    public int DriverId { get; set; }

    public TimeOnly DepartureTime { get; set; }

    public TimeOnly ArrivalTime { get; set; }

    public virtual Bus Bus { get; set; } = null!;

    public virtual Driver Driver { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual Route Route { get; set; } = null!;
}
