using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Models;

namespace CroweAlumniPortal.Data.IServices
{
    public interface INotificationService
    {
        Task<int> CreateAsync(Notification n, IEnumerable<int> userIds);
        Task<int> CreateForUserAsync(Notification n, int userId);
        Task<int> CreateForAllAsync(Notification n, int? exceptUserId = null);
        Task<List<object>> GetUnreadAsync(int userId, int take = 20);
        Task MarkReadAsync(int notifUserId, int userId);
        Task<int> CountUnreadAsync(int userId);
        Task NotifyAdminsNewUserAsync(User user);
        Task NotifyUserApprovedAsync(User user);
        Task NotifyUserRejectedAsync(User user, string reason);
        Task NotifyPostSoftDeletedAsync(User author, Post post, int admin, string adminName);
        Task<List<NotificationListItemDto>> GetAllForUserAsync(int userId);
        Task MarkAsReadAsync(int notificationUserId, int userId);
    }
}
