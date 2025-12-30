using System.IO;
using System.Xml.Linq;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IChatService chat;
        private readonly IUnitOfWork uow;                 
        private readonly INotificationService notificationService;
        private readonly IFileService fileService;
        private readonly IHubContext<ChatHub> hub;
        private readonly IMailService mailService ;
        public ChatController(IChatService chat, IUnitOfWork uow, IFileService fileService, INotificationService notificationService, IHubContext<ChatHub> hub, IMailService mailService)
        {
            this.chat = chat;
            this.uow = uow;
            this.fileService = fileService;
            this.notificationService = notificationService;
            this.hub = hub;
            this.mailService = mailService;
        }

        [HttpPost("start/{otherId:int}")]
        public async Task<IActionResult> Start(int otherId)
        {
            var me = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(me, out var myId)) return Unauthorized();

            var c = await chat.StartOrGet1to1Async(myId, otherId);
            return Ok(new { c.Id });
        }

        /*[HttpGet("{conversationId:int}/messages")]
        public async Task<IActionResult> Messages(int conversationId, int take = 50, int? beforeId = null)
        {
            var list = await chat.GetMessagesAsync(conversationId, take, beforeId);
            return Ok(list.Select(m => new
            {
                m.Id,
                m.Body,
                m.MediaPath,
                m.MediaType,
                m.SenderId,
                m.CreatedOn
            }));
        }*/

        [HttpGet("{conversationId:int}/messages")]
        public async Task<IActionResult> Messages(int conversationId, int take = 50, int? beforeId = null)
        {
            var list = await chat.GetMessagesAsync(conversationId, take, beforeId); 

            var senderIds = list.Select(m => m.SenderId).Distinct().ToList();
            var users = await uow.UserService.ListByIdsAsync(senderIds);
            var map = users.ToDictionary(
                u => u.Id,
                u => new {
                    Name = $"{(u.FirstName ?? "").Trim()} {(u.LastName ?? "").Trim()}".Trim(),
                    Avatar = u.ProfilePicturePath 
                }
            );

            return Ok(list.Select(m => new {
                m.Id,
                m.Body,
                m.MediaPath,
                m.MediaType,
                m.SenderId,
                m.CreatedOn,
                senderName = map.TryGetValue(m.SenderId, out var u) ? (u?.Name ?? $"User {m.SenderId}") : $"User {m.SenderId}",
                senderAvatar = map.TryGetValue(m.SenderId, out var u2) ? u2?.Avatar : null
            }));
        }


        [HttpPost("{conversationId:int}/send")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Send(int conversationId, [FromForm] string body, [FromForm] IFormFile? media)
        {
            var me = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(me, out var myId)) return Unauthorized();
            string? path = null; string? type = null;
            if (media != null && media.Length > 0)
            {
                type = media.ContentType.StartsWith("image/") ? "image" : "file";
                path = await fileService.SaveFileAsync(media, "assets/img/uploads/chat");
            }
            var msg = await chat.SendAsync(conversationId, myId, body, type, path);
            var memberIds = (await uow.ChatService.ListForUserAsync(myId))
                 .FirstOrDefault(c => c.Id == conversationId)?
                 .Members?.Select(m => m.UserId)
                 .Where(id => id != myId)
                 .Distinct()
                 .ToList() ?? new List<int>();

            var meUser = await uow.UserService.Get(myId);
            var meName = $"{meUser?.FirstName} {meUser?.LastName}".Trim();
            var inboxLink = $"{Request.Scheme}://{Request.Host}/Message/Inbox?c={conversationId}";
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var convoUrl = $"{baseUrl}/Message/Inbox?c={conversationId}";
            var preview = string.IsNullOrWhiteSpace(body) ? "Sent an attachment" : body;
            if (memberIds.Count > 0)
            {
                await notificationService.CreateAsync(
                    new Notification
                    {
                        Type = "message",
                        Title = "New Message",
                        Message = $"{meName}: {(string.IsNullOrWhiteSpace(body) ? "Sent an attachment" : body)}",
                        Url = $"/Message/Inbox?c={conversationId}"
                    },
                    memberIds
                );
                foreach(var id in memberIds)
                {
                    var receivers = await uow.UserService.Get(id); // implement if not present
                    
                        if (string.IsNullOrWhiteSpace(receivers?.EmailAddress)) continue;

                        var receiverName = $"{receivers.FirstName} {receivers.LastName}".Trim();
                        var subject = EmailTemplates.NewMessageSubject(meName);
                        var html = EmailTemplates.NewMessageBody(receiverName, meName, preview, convoUrl);

                       // await mailService.SendEmailAsync(receivers.EmailAddress, subject, html);
                        await mailService.SendEmailAsync(new MailRequestDto { ToEmail = receivers.EmailAddress, Subject = subject, Body = html });


                }
                
            }
            return Ok(new { msg.Id });
        }

        /*[HttpPost("{conversationId:int}/seen")]
        public async Task<IActionResult> Seen(int conversationId)
        {
            var me = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(me, out var myId)) return Unauthorized();

            await chat.MarkSeenAsync(conversationId, myId);
            await hub.Clients.Group($"convo-{conversationId}").SendAsync("seen", new { userId = myId, conversationId });
            return Ok();
        }*/
        [HttpPost("{conversationId:int}/seen")]
        [Produces("application/json")]
        public async Task<IActionResult> Seen(int conversationId)
        {
            var me = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(me, out var myId)) return Unauthorized();

            await chat.MarkSeenAsync(conversationId, myId);
            return Ok(new { ok = true, conversationId, seenOn = DateTime.UtcNow });
        }

        [HttpGet("unread/count")]
        public async Task<IActionResult> UnreadCount()
        {
            var me = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(me, out var myId)) return Unauthorized(); // returns 401, not 500
            var n = await chat.CountUnreadAsync(myId);
            return Ok(n);
        }

        // GET /api/chat/list

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var meStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(meStr, out var me)) return Unauthorized();

            var convos = await uow.ChatService.ListForUserAsync(me); 

            // sidebar projection
            var data = convos.Select(c =>
            {
                var others = c.Members.Where(m => m.UserId != me).Select(m => m.User).ToList();
                var lastMsg = c.Messages.OrderByDescending(m => m.CreatedOn).FirstOrDefault();
                var unread = c.Messages.Any(m => m.SenderId != me &&
                                                 (c.Members.First(x => x.UserId == me).LastSeenOn == null ||
                                                  m.CreatedOn > c.Members.First(x => x.UserId == me).LastSeenOn));
                string title;
                string? avatar;

                if (c.IsGroup == true)
                {
                    title = string.IsNullOrWhiteSpace(c.Title) ? "Group" : c.Title!;
                    avatar = null;
                }
                else
                {
                    var o = others.FirstOrDefault();
                    title = (o?.FirstName + " " + o?.LastName).Trim();
                    avatar = o?.ProfilePicturePath; 
                }

                return new
                {
                    c.Id,
                    Title = title,
                    Avatar = avatar,
                    IsGroup = c.IsGroup,
                    LastMessage = lastMsg?.Body ?? (lastMsg?.MediaPath != null ? "📎 Attachment" : ""),
                    LastAt = lastMsg?.CreatedOn,
                    HasUnread = unread
                };
            })
            .OrderByDescending(x => x.LastAt) // recent on top
            .ToList();

            return Ok(data);
        }

        // GET /api/chat/{conversationId}/header 
        [HttpGet("{conversationId:int}/header")]
        public async Task<IActionResult> Header(int conversationId)
        {
            var meStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(meStr, out var me)) return Unauthorized();

            var convos = await uow.ChatService.ListForUserAsync(me);
            var c = convos.FirstOrDefault(x => x.Id == conversationId);
            if (c == null) return NotFound();

            if (!c.IsGroup)
            {
                var other = c.Members.Select(m => m.User).First(u => u.Id != me);
                return Ok(new
                {
                    Title = (other.FirstName + " " + other.LastName).Trim(),
                    Avatar = other.ProfilePicturePath,
                    Sub = "Online status (optional)"
                });
            }
            else
            {
                return Ok(new
                {
                    Title = string.IsNullOrWhiteSpace(c.Title) ? "Group" : c.Title!,
                    Avatar = (string?)null,
                    Sub = $"{c.Members.Count} members"
                });
            }
        }

    }
}
