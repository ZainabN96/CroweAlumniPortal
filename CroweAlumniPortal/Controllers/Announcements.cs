using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers
{
    public class Announcements : Controller
    {
       
        public IActionResult AddAnnouncement()
        {
            return View();
        }
    }
}
