using System;
using System.Collections.Generic;

namespace IntercityTransportManagementSystem.Models;

public partial class Route
{
    public int Id { get; set; }

    public string StartDestination { get; set; } = null!;

    public string FinalDestination { get; set; } = null!;

    public virtual ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
}
