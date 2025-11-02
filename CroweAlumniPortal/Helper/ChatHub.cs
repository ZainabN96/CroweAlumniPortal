using Microsoft.AspNetCore.SignalR;

namespace CroweAlumniPortal.Helper
{
    public class ChatHub: Hub
    {
        public Task JoinConversation(int conversationId)
        => Groups.AddToGroupAsync(Context.ConnectionId, $"convo-{conversationId}");

        public Task LeaveConversation(int conversationId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"convo-{conversationId}");
    }
}
