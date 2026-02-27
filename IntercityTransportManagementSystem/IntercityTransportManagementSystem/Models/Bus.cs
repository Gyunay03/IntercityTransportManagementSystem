using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Models;

public partial class Bus
{
    public int Id { get; set; }
    
    [Display(Name = "Регистрационен номер")]
    public string RegistrationNumber { get; set; } = null!;
    
    [Display(Name = "Капацитет")]
    public int Capacity { get; set; }

    public virtual ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();

    public virtual ICollection<BusSeat> BusSeats { get; set; } = new List<BusSeat>();
}
