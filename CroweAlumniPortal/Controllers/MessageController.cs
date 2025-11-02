using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers
{
    public class MessageController : Controller
    {
        public IActionResult Inbox(int c)
        {
            ViewBag.ConversationId = c;
            return View();
        }
    }
}
