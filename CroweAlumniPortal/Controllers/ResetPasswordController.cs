using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers
{
    public class ResetPasswordController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
