using CroweAlumniPortal.Data.Services;

namespace CroweAlumniPortal.Data.IServices
{
    public interface IUnitOfWork
    {
        IUserService UserService { get; }
        IEventService EventService { get; }
        IAlumniService AlumniService { get; }
        IPostService PostService { get; }
        INotificationService NotificationService { get; }
       IChatService ChatService { get; }
        Task<bool> SaveAsync();
    }
}
