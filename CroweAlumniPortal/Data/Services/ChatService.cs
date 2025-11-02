using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace CroweAlumniPortal.Data.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext dc;
        public ChatService(ApplicationDbContext dc)
        {
            this.dc = dc; 
        }
        public async Task<Conversation> StartOrGet1to1Async(int userId, int otherUserId)
        {
            // try to find existing 1-1
            var convo = await dc.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => !c.IsGroup
                                          && c.Members.Count == 2
                                          && c.Members.Any(m => m.UserId == userId)
                                          && c.Members.Any(m => m.UserId == otherUserId));
            if (convo != null) return convo;

            // create new
            convo = new Conversation(); 
            convo.Members.Add(new ConversationMember { UserId = userId });
            convo.Members.Add(new ConversationMember { UserId = otherUserId });

            dc.Conversations.Add(convo);
            await dc.SaveChangesAsync();
            return convo;
        }

        public async Task<List<Conversation>> ListForUserAsync(int userId) {
            return await dc.Conversations
                           .AsNoTracking()
                           .Include(c => c.Members).ThenInclude(m => m.User)
                           .Where(c => c.Members.Any(m => m.UserId == userId))
                           .Select(c => new {
                                Conv = c,
                                LastMsgAt = dc.Messages
                                .Where(m => m.ConversationId == c.Id)
                                .Select(m => (DateTime?)m.CreatedOn)
                                .OrderByDescending(x => x)
                                .FirstOrDefault()
                           })
                           .OrderByDescending(x => x.LastMsgAt)
                           .Select(x => x.Conv)
                           .ToListAsync();
        }

        public async Task<List<Message>> GetMessagesAsync(int conversationId, int take = 50, int? beforeId = null)
        {
            var q = dc.Messages.AsNoTracking()
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted);

            if (beforeId.HasValue)
                q = q.Where(m => m.Id < beforeId.Value);

            q = q.OrderByDescending(m => m.Id); // <— order last

            return await q.Take(take).ToListAsync();
        }

        public async Task<Message> SendAsync(int conversationId, int senderId, string body, string type, string path)
        {
            var msg = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Body = (body ?? "").Trim(),
                MediaPath = path,
                MediaType = type,
                CreatedBy = senderId,
                CreatedOn = DateTime.UtcNow
            };
            dc.Messages.Add(msg);
            await dc.SaveChangesAsync();
            return msg;
        }

        public async Task MarkSeenAsync(int conversationId, int userId)
        {
            var cm = await dc.ConversationMembers
                .FirstOrDefaultAsync(x => x.ConversationId == conversationId && x.UserId == userId);
            if (cm != null)
            {
                cm.LastSeenOn = DateTime.UtcNow;
                await dc.SaveChangesAsync();
            }
        }
        public async Task<int> CountUnreadAsync(int userId)
        {
            var myConvos = dc.ConversationMembers.Where(m => m.UserId == userId);

            var unread = from m in dc.Messages
                         join cm in myConvos on m.ConversationId equals cm.ConversationId
                         where m.SenderId != userId
                               && !m.IsDeleted
                               && (cm.LastSeenOn == null || m.CreatedOn > cm.LastSeenOn)
                         select m.Id;

            return await unread.CountAsync();
        }
    }

}
