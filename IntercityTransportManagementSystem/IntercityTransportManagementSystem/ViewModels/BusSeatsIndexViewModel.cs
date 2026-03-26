using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class BusSeatsIndexViewModel
    {
        public IEnumerable<BusSeat> BusSeats { get; set; }
        public IEnumerable<string> RegistrationNumbers { get; set; }
        public string SearchString { get; set; }
        public string SortOrder { get; set; }
        public string RegistrationNumberFilter { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
