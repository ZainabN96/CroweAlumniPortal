using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Models;

namespace CroweAlumniPortal.Data.IServices
{
    public interface IUserService
    {
        User Register(User user);
        Task<User> Authenticate(string loginId, string password);
        Task<User> RegisterAsync(User user);
        Task<User> Get(int Id);
        Task<List<User>> ListByIdsAsync(IEnumerable<int> ids);
        Task<User?> GetByIdAsync(long id);
        Task<User?> GetByEmailAsync(string email);
        Task<int> CountAsync(UserApprovalStatus? status = null);
        Task<PagedResult<PendingUserRow>> GetUsersByStatusAsync(UserApprovalStatus status, int page, int pageSize);
        Task<List<string>> GetApprovedUserEmailsAsync();
        Task<IEnumerable<User>> ListAllAsync();
    }
    public sealed class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
    }

    public sealed class PendingUserRow
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string EmailAddress { get; set; } = "";
        public string LoginId { get; set; } = "";
        public string MemberStatus { get; set; } = "";
        public string Qualification { get; set; } = "";
        public string ProfilePicturePath { get; set; } = "";
    }
}