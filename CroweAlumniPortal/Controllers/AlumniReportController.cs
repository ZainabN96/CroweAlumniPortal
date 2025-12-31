using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers
{
    public class AlumniReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
