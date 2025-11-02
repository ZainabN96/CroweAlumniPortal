using CroweAlumniPortal.Data;
using CroweAlumniPortal.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CroweAlumniPortal.Models;           
using System.Linq;

namespace CroweAlumniPortal.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext dc;

        public DashboardController(ApplicationDbContext dc)
        {
            this.dc = dc;
        }
        public async Task<IActionResult> dashboard()
        {
            var now = DateTime.Now;

            // sirf active events
            var baseQuery = dc.Events.AsNoTracking().Where(e => e.IsActive);

            var ongoing = await baseQuery
                .Where(e => e.StartDateTime <= now && e.EndDateTime >= now)
                .OrderBy(e => e.EndDateTime)
                .Take(6)
                .ToListAsync();

            var upcoming = await baseQuery
                .Where(e => e.StartDateTime > now)
                .OrderBy(e => e.StartDateTime)
                .Take(6)
                .ToListAsync();

            var past = await baseQuery
                .Where(e => e.EndDateTime < now)
                .OrderByDescending(e => e.EndDateTime)
                .Take(6)
                .ToListAsync();

            var vm = new EventsDashboardVm
            {
                Ongoing = ongoing,
                Upcoming = upcoming,
                Past = past
            };
            return View(vm);
        }
        public async Task<IActionResult> Eventsdetail(int id)
        {
            var ev = await dc.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (ev == null) return NotFound();
            return View(ev);
        }
        public async Task<IActionResult> Noticeboard()
        {
            var now = DateTime.Now;

            // sirf active events
            var baseQ = dc.Events.AsNoTracking().Where(e => e.IsActive);

            var ongoing = await baseQ
                .Where(e => e.StartDateTime <= now && e.EndDateTime >= now)
                .OrderBy(e => e.EndDateTime)
                .Take(6)
                .ToListAsync();

            var upcoming = await baseQ
                .Where(e => e.StartDateTime > now)
                .OrderBy(e => e.StartDateTime)
                .Take(6)
                .ToListAsync();

            var past = await baseQ
                .Where(e => e.EndDateTime < now)
                .OrderByDescending(e => e.EndDateTime)
                .Take(6)
                .ToListAsync();

            var vm = new EventsDashboardVm
            {
                Ongoing = ongoing,
                Upcoming = upcoming,
                Past = past
            };
            return View(vm);
        }

    }
}
