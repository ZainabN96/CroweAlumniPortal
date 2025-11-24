using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers
{
    //[Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetUserId();
            var list = await _notificationService.GetAllForUserAsync(userId.Value);
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = HttpContext.Session.GetUserId();
            if (userId == null)
            {
                return Unauthorized("User is not found");
            }
            await _notificationService.MarkReadAsync(id, userId.Value);
            return Ok();
        }
    }
}
