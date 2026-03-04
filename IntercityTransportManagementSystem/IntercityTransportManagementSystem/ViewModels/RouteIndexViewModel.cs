using Route = IntercityTransportManagementSystem.Models.Route;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class RouteIndexViewModel
    {
        public IEnumerable<Route> Routes { get; set; }
        public string SearchString { get; set; }
        public string SortOrder { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}