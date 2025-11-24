using AutoMapper;
using CroweAlumniPortal.Data.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : Controller
    {
        private IUnitOfWork uow;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        public NotificationController(IUnitOfWork uow, IMapper mapper, INotificationService notificationService)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.notificationService = notificationService;
        }

        [HttpGet("user/{userId}/unread")]
        public async Task<IActionResult> GetUnread(int userId, int take)
        {
            var list = await uow.NotificationService.GetUnreadAsync(userId, take);
            return Ok(list);
        }
    }
}
