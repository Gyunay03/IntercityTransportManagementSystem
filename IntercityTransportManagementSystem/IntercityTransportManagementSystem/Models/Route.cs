using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Models;

public partial class Route
{
    public int Id { get; set; }

    [Display(Name = "Тръгване от")]
    public string StartDestination { get; set; } = null!;

    [Display(Name = "Пристигане в")]
    public string FinalDestination { get; set; } = null!;

    [Display(Name = "Разстояние")]
    public double Distance { get; set; }
    
    [Display(Name = "Време за пътуване")]
    public TimeSpan EstimatedDuration { get; set; }
    public virtual ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
}
