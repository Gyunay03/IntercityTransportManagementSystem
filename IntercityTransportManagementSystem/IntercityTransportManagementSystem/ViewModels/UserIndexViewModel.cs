using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.ViewModels
{
    public class UserIndexViewModel
    {
        public List<User> Users { get; set; }
        public string SearchString { get; set; }
        public string RoleFilter { get; set; }
        public bool? IsActive { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SortOrder { get; set; }
    }
}
