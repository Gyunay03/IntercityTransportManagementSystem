using System;
using System.Collections.Generic;

namespace IntercityTransportManagementSystem.Models;

public partial class Bus
{
    public int Id { get; set; }

    public string RegistrationNumber { get; set; } = null!;

    public int Capacity { get; set; }

    public virtual ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();

    public virtual ICollection<BusSeat> BusSeats { get; set; } = new List<BusSeat>();
}
