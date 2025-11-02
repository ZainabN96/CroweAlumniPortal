namespace CroweAlumniPortal.Helper
{
    public interface Interface
    {
    }
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body);
    }

    public interface IAdminDirectory
    {
        Task<List<string>> GetAllAdminEmailsAsync();
        Task<List<int>> GetAllAdminIdsAsync();
    }
}
