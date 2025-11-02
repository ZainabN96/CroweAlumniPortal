using CroweAlumniPortal.Models;

namespace CroweAlumniPortal.Data.IServices
{
    public interface IChatService
    {
        Task<Conversation> StartOrGet1to1Async(int userId, int otherUserId);
        Task<List<Conversation>> ListForUserAsync(int userId);

        Task<List<Message>> GetMessagesAsync(int conversationId, int take = 50, int? beforeId = null);
        Task<Message> SendAsync(int conversationId, int senderId, string body, string type, string path);
        Task MarkSeenAsync(int conversationId, int userId);
        Task<int> CountUnreadAsync(int userId);
    }
}
