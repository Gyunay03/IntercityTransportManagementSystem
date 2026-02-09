using System;
using System.Collections.Generic;

namespace IntercityTransportManagementSystem.Models;

public partial class Driver
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
}
