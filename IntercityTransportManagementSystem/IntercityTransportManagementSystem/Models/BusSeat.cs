using System;
using System.Collections.Generic;

namespace IntercityTransportManagementSystem.Models;

public partial class BusSeat
{
    public int Id { get; set; }

    public int Number { get; set; }

    public int BusId { get; set; }

    public virtual Bus Bus { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
