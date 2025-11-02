using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers
{
    public class Alumni : Controller
    {
        public IActionResult AlumniForm()
        {
            return View();
        }
        public IActionResult Findalumni()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult RegistrationRequest() 
        { 
            return View();
        }

    }
}
