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

        private int GetCurrentUserId()
        {
            // apne project ke mutabiq adjust karo:
            // agar tum JWT / Claims mein "UserId" store karti ho:
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (int.TryParse(idClaim, out var id))
                return id;

            // worst case fallback (debug ke liye)
            throw new Exception("UserId claim not found.");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //var userId = GetCurrentUserId();
            var userId = HttpContext.Session.GetUserId();
            var list = await _notificationService.GetAllForUserAsync(userId.Value);
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = HttpContext.Session.GetUserId();
            await _notificationService.MarkReadAsync(id, userId.Value);
            return Ok();
        }
    }
}
