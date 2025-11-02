using AutoMapper;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : Controller
    {
        private IUnitOfWork uow;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        public EventController(IUnitOfWork uow, IMapper mapper, INotificationService notificationService)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.notificationService = notificationService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] EventDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Session.GetUserId();

            var created = await uow.EventService.CreateAsync(dto, userId);
            var author = userId.HasValue ? await uow.UserService.Get(userId.Value) : null;
            var authorName = (author == null)
                ? "Someone"
                : $"{author.FirstName ?? ""} {author.LastName ?? ""}".Trim();

            var relativeUrl = $"/Dashboard/Eventsdetail?id={created.Id}";

            await notificationService.CreateForAllAsync(new Notification
            {
                Type = "event",
                Title = "New Event Added",
                Message = $"{authorName} added a new event: {dto.Title ?? "Untitled"}",
                Url = relativeUrl
            }, exceptUserId: userId);

            return Ok(created);
        }

    }
}
