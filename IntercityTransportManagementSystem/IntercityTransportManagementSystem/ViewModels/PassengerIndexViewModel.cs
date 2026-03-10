using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class PassengerIndexViewModel
    {
        public IEnumerable<Passenger> Passengers { get; set; }
        public string SearchString { get; set; }
        public string SortOrder { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
