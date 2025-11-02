using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace CroweAlumniPortal.Data.Services
{
    public class UserService: IUserService
    {
        private readonly ApplicationDbContext dc;

        public UserService(ApplicationDbContext dc)
        {
            this.dc = dc;
        }

        public async Task<User> Authenticate(string loginId, string password)
        {
            var user = await dc.Users.FirstOrDefaultAsync(x => x.LoginId == loginId && x.IsActive == true);

            if (user == null )
                return null;

            if (!string.Equals(user.Password, password))
                return null;

            return user;
        }

        private static string GetOrgPrefix(string? orgType)
        {
            if (string.IsNullOrWhiteSpace(orgType)) return "ALM";
            return orgType.Trim().ToUpperInvariant() switch
            {
                "CROWE" => "CRW",
                "Crowe" => "Crowe",
                _ => "ALM"
            };
        }
        private async Task<string> GenerateOrgYearCounterLoginIdAsync(User user)
        {
            var prefix = GetOrgPrefix(user.OrganizationType);
            var yy = user.JoiningDate?.Year.ToString() ?? DateTime.UtcNow.Year.ToString();
            var basePrefix = $"{prefix}-{yy}-"; 

            var last = await dc.Users
                .Where(u => u.LoginId.StartsWith(basePrefix))
                .Select(u => u.LoginId)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            int next = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var parts = last.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3 && int.TryParse(parts[2], out var n))
                    next = n + 1;
            }

            return $"{basePrefix}{next:D4}";
        }
        public User Register(User user)
        {
            user.IsActive = true;
            user.LastUpdatedOn = DateTime.Now;

            dc.Users.Add(user);
            return user;
        }
        public async Task<User> RegisterAsync(User user)
        {
            user.IsActive = false;
            user.LastUpdatedOn = DateTime.UtcNow;
            user.CreatedAt = DateTime.UtcNow;

            if (await dc.Users.AnyAsync(x => x.EmailAddress == user.EmailAddress))
                throw new InvalidOperationException("Email already exists.");

            if (await dc.Users.AnyAsync(x => x.CNIC == user.CNIC))
                throw new InvalidOperationException("CNIC already exists.");

            var isAdmin = string.Equals(user.UserType, "Admin", StringComparison.OrdinalIgnoreCase);
            
            if (!isAdmin)
            {
                user.LoginId = await GenerateOrgYearCounterLoginIdAsync(user);
            }
            else
            {
                var joinYear = user.JoiningDate?.Year.ToString() ?? DateTime.UtcNow.Year.ToString();

                var basePrefix = $"ADM-{joinYear}-"; 

                var last = await dc.Users
                    .Where(u => u.LoginId.StartsWith(basePrefix))
                    .Select(u => u.LoginId)
                    .OrderByDescending(x => x)
                    .FirstOrDefaultAsync();

                int next = 1;
                if (!string.IsNullOrEmpty(last))
                {
                    var parts = last.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4 && int.TryParse(parts[3], out var n))
                        next = n + 1;
                }

                user.LoginId = $"{basePrefix}{next:D4}";
            }

            dc.Users.Add(user);
            try
            {
                await dc.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!isAdmin)
                {
                    user.LoginId = await GenerateOrgYearCounterLoginIdAsync(user);
                    await dc.SaveChangesAsync();
                }
                else throw;
            }

            return user;
        }

        public async Task<User> Get(int id)
        {
            return await dc.Users.Where(x => x.Id == id)
                                 .FirstOrDefaultAsync();
        }

        public async Task<List<User>> ListByIdsAsync(IEnumerable<int> ids)
        {
            return await dc.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
        }

        public Task<User?> GetByIdAsync(long id) =>
           dc.Users.FirstOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByEmailAsync(string email) =>
            dc.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);

        public async Task<IServices.PagedResult<PendingUserRow>> GetUsersByStatusAsync(UserApprovalStatus status, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var baseQuery = dc.Users.AsNoTracking().Where(u => u.ApprovalStatus == status);

            var total = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new PendingUserRow
                {
                    Id = u.Id,
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    EmailAddress = u.EmailAddress ?? "",
                    LoginId = u.LoginId ?? "",
                    MemberStatus = u.MemberStatus ?? "",
                    Qualification = u.Qualification ?? "",
                    ProfilePicturePath = u.ProfilePicturePath ?? ""
                })
                .ToListAsync();

            return new IServices.PagedResult<PendingUserRow> { Items = items, Total = total };
        }
        public Task<int> CountAsync(UserApprovalStatus? status = null)
        {
            IQueryable<User> q = dc.Users.AsNoTracking();
            if (status.HasValue)
                q = q.Where(u => u.ApprovalStatus == status.Value && u.UserType == "Alumni");
            return q.CountAsync();
        }
    }
}
