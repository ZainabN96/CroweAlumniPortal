using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CroweAlumniPortal.Data.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext dc;
        private readonly IHubContext<NotificationHub> hub;
        private readonly IEmailSender emailSender;
        private readonly IAdminDirectory adminDirectory;

        public NotificationService(ApplicationDbContext dc, IHubContext<NotificationHub> hub, IEmailSender emailSender, IAdminDirectory adminDirectory)
        {
            this.dc = dc;
            this.hub = hub;
            this.emailSender = emailSender;
            this.adminDirectory = adminDirectory;
        }
        public async Task<int> CreateAsync(Notification n, IEnumerable<int> userIds)
        {
            n.CreatedOn = DateTime.UtcNow;
            dc.Notifications.Add(n);
            await dc.SaveChangesAsync();

            var distinctIds = userIds.Distinct().ToList();

            var rows = distinctIds.Select(uid => new NotificationUser
            {
                NotificationId = n.Id,
                UserId = uid
            }).ToList();

            dc.NotificationUsers.AddRange(rows);
            await dc.SaveChangesAsync();

            var userKeys = distinctIds.Select(i => i.ToString()).ToList();
            await hub.Clients.Users(userKeys).SendAsync("notify", new
            {
                id = n.Id,
                n.Type,
                n.Title,
                n.Message,
                n.Url,
                n.CreatedOn
            });

            return n.Id;
        }
        public Task<int> CreateForUserAsync(Notification n, int userId)
            => CreateAsync(n, new[] { userId });

        public async Task<int> CreateForAllAsync(Notification n, int? exceptUserId = null)
        {
            var allIds = await dc.Users.Select(u => u.Id).ToListAsync();
            if (exceptUserId.HasValue) allIds = allIds.Where(i => i != exceptUserId.Value).ToList();
            n.IsGlobal = true;
            return await CreateAsync(n, allIds);
        }

        public async Task<List<object>> GetUnreadAsync(int userId, int take)
        {
            return await dc.NotificationUsers
                .AsNoTracking()
                .Where(nu => nu.UserId == userId && !nu.IsRead)
                .OrderByDescending(nu => nu.Id)
                .Take(take)
                .Select(nu => new {
                    notifUserId = nu.Id,
                    nu.Notification.Id,
                    nu.Notification.Type,
                    nu.Notification.Title,
                    nu.Notification.Message,
                    nu.Notification.Url,
                    nu.Notification.CreatedOn
                })
                .Cast<object>()
                .ToListAsync();
        }
        public async Task MarkReadAsync(int notifUserId, int userId)
        {
            var row = await dc.NotificationUsers
                .FirstOrDefaultAsync(x => x.Id == notifUserId && x.UserId == userId);
            if (row != null && !row.IsRead)
            {
                row.IsRead = true; row.ReadOn = DateTime.UtcNow;
                await dc.SaveChangesAsync();
            }
        }

        public Task<int> CountUnreadAsync(int userId)
            => dc.NotificationUsers.CountAsync(x => x.UserId == userId && !x.IsRead);
        public async Task NotifyAdminsNewUserAsync(User user)
        {
            var adminEmails = await adminDirectory.GetAllAdminEmailsAsync();
            var subject = "New Alumni Registration Pending Approval";
            var body = $@"A new user has registered and is awaiting approval:<br><br>
                  Name: {user.FirstName} {user.LastName}<br>
                  Email: {user.EmailAddress}<br>
                  Member: {user.MemberStatus}<br>
                  Qualification: {user.Qualification}<br>
                  Approve: https://localhost:44378/Alumni/RegistrationRequest";

            foreach (var addr in adminEmails)
                _ = emailSender.SendAsync(addr, subject, body); 

            var adminIds = await adminDirectory.GetAllAdminIdsAsync(); 
            var notif = new Notification
            {
                Type = "info",
                Title = "New Alumni Registration",
                Message = $"{user.FirstName} {user.LastName} just registered and awaits approval.",
                Url = $"/Alumni/RegistrationRequest"
            };
            await CreateAsync(notif, adminIds);

            await hub.Clients.Group("admins").SendAsync("notify", new
            {
                id = notif.Id,
                notif.Type,
                notif.Title,
                notif.Message,
                notif.Url,
                notif.CreatedOn
            });

            await hub.Clients.Group("admins").SendAsync("NewUserPending", new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.EmailAddress
            });
        }

        public Task NotifyUserApprovedAsync(User user) =>
            emailSender.SendAsync(user.EmailAddress,
                "Your Alumni Account is Approved",
                "Congratulations! Your alumni account has been approved. You can now sign in.");
        public Task NotifyUserRejectedAsync(User user, string reason) =>
            emailSender.SendAsync(user.EmailAddress,
                Helper.EmailTemplates.RejectedSubject,
                Helper.EmailTemplates.RejectedBody(user.FirstName, user.LastName, reason));

        public async Task NotifyPostSoftDeletedAsync(User author, Post post, int admin, string adminName)
        {
            var notification = new Notification
            {
                Type = "post-deleted",
                Title = "Your post was removed by admin",
                Message = $"Your post \"{post?.Title ?? "Untitled"}\" has been removed by {adminName}.",
                Url = $"/Posts/Details/{post?.Id}",
                IsGlobal = false,
                CreatedBy = admin,
                CreatedOn = DateTime.UtcNow
            };

            // 2️⃣ Add notification record
            dc.Notifications.Add(notification);
            await dc.SaveChangesAsync();

            // 3️⃣ Create user mapping
            var userNotification = new NotificationUser
            {
                NotificationId = notification.Id,
                UserId = author.Id,
                IsRead = false
            };

            dc.NotificationUsers.Add(userNotification);
            await dc.SaveChangesAsync();

            // 4️⃣ Optional real-time push (SignalR)
            await hub.Clients.Group($"user-{author.Id}").SendAsync("PostSoftDeleted", new
            {
                title = notification.Title,
                message = notification.Message,
                postId = post?.Id,
                admin = adminName,
                createdOn = notification.CreatedOn
            });
        }
        public async Task<List<NotificationListItemDto>> GetAllForUserAsync(int userId)
        {
            return await dc.Set<NotificationUser>()
                .AsNoTracking()
                .Include(nu => nu.Notification)
                .Where(nu => nu.UserId == userId)
                .OrderByDescending(nu => nu.Notification.CreatedOn) // ya jo bhi field ho
                .Select(nu => new NotificationListItemDto
                {
                    NotificationUserId = nu.Id,
                    NotificationId = nu.NotificationId,
                    Type = nu.Notification.Type,
                    Title = nu.Notification.Title,
                    Message = nu.Notification.Message,
                    Url = nu.Notification.Url,
                    IsRead = nu.IsRead,
                    CreatedOn = nu.Notification.CreatedOn,  // adjust
                    ReadOn = nu.ReadOn
                })
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationUserId, int userId)
        {
            var entity = await dc.Set<NotificationUser>()
                .FirstOrDefaultAsync(x => x.Id == notificationUserId && x.UserId == userId);

            if (entity == null) return;

            if (!entity.IsRead)
            {
                entity.IsRead = true;
                entity.ReadOn = DateTime.UtcNow;
                await dc.SaveChangesAsync();
            }
        }
    }

}

