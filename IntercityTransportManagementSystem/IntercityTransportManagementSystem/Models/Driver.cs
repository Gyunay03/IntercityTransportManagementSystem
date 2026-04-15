using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Models;

public partial class Driver
{
    public int Id { get; set; }

    [Display(Name = "Име")]
    public string Name { get; set; } = null!;

    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Имейл адрес")]
    public string Email { get; set; } = null!;

    [Display(Name = "Телефонен номер")]
    public string PhoneNumber { get; set; } = null!;

    [Display(Name = "Номер на шофьорска книжка")]
    public string LicenseNumber { get; set; } = null!;

    [Display(Name = "Дата на назначаване")]
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }
    public virtual ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
}
