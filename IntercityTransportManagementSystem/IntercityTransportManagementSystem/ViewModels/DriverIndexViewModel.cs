using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class DriverIndexViewModel
    {
        public IEnumerable<Driver> Drivers { get; set; }
        public string SearchString { get; set; }
        public string SortOrder { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}