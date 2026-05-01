using Hangfire.Dashboard;

namespace IntercityTransportManagementSystem.Filters
{
    public class HangfireAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            return httpContext.User.Identity.IsAuthenticated &&
                   httpContext.User.IsInRole("Administrator");
        }
    }
}
