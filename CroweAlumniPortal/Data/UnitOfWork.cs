using System.Security;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Data.Services;
using CroweAlumniPortal.Helper;          
using Microsoft.AspNetCore.SignalR;

namespace CroweAlumniPortal.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dc;
        private readonly IHubContext<NotificationHub> hub;
        private readonly IEmailSender emailSender;
        private readonly IAdminDirectory adminDirectory;
        public UnitOfWork(ApplicationDbContext dc, IHubContext<NotificationHub> hub, IEmailSender emailSender, IAdminDirectory adminDirectory)
        {
            this.dc = dc;
            this.hub = hub;
            this.emailSender = emailSender; 
            this.adminDirectory = adminDirectory;
        }
        public IUserService UserService => new UserService(dc);
        public IEventService EventService => new EventService(dc);
        public IAlumniService AlumniService => new AlumniService(dc);
        public IPostService PostService => new PostService(dc);
        public INotificationService NotificationService => new NotificationService(dc, hub, emailSender, adminDirectory);
        public IChatService ChatService => new ChatService(dc);
        public async Task<bool> SaveAsync()
        {
            try
            {
                return await dc.SaveChangesAsync() > 0;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
