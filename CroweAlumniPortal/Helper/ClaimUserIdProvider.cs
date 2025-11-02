using Microsoft.AspNetCore.SignalR;

namespace CroweAlumniPortal.Helper
{
    public class ClaimUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        =>connection.User?.FindFirst("UserId")?.Value;
    }
}
