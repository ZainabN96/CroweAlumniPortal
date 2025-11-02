using CroweAlumniPortal.Models;

namespace CroweAlumniPortal.Helper
{
    public class EventsDashboardVm
    {
        public List<Event> Ongoing { get; set; } = new();
        public List<Event> Upcoming { get; set; } = new();
        public List<Event> Past { get; set; } = new();
    }
}
