using IntercityTransportManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class BusScheduleIndexViewModel
    {
        public IEnumerable<BusSchedule> BusSchedules { get; set; }
        public string SearchString { get; set; }
        public string SortOrder { get; set; }
        public DateOnly? TravelDate { get; set; }
        [FromQuery]
        public TimeOnly? DepartureTime { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
