using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Dtos;      
using CroweAlumniPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace CroweAlumniPortal.Data.Services
{
    public class AlumniService : IAlumniService
    {
        private readonly ApplicationDbContext _dc;
        public AlumniService(ApplicationDbContext dc) => _dc = dc;

        public async Task<IEnumerable<User>> GetAllAlumniAsync()
        {
            return await _dc.Users.Where(x => x.UserType == "Alumni" && x.IsActive == true)
                                       .OrderBy(x => x.FirstName)
                                       .ToListAsync();

        }

        public async Task<Dtos.PagedResult<AlumniCardDto>> GetAlumniAsync(string? search, int page, int pageSize)
        {
            var q = _dc.Users.AsNoTracking()
                .Where(u => u.UserType == "Alumni" && u.IsActive == true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(u =>
                    ((u.FirstName ?? "").ToLower().Contains(s)) ||
                    ((u.LastName ?? "").ToLower().Contains(s)) ||
                    ((u.City ?? "").ToLower().Contains(s)) ||
                    ((u.Country ?? "").ToLower().Contains(s))
                );
            }

            var total = await q.CountAsync();

            var items = await q
                .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AlumniCardDto
                {
                    Id = u.Id,
                    FullName = string.Join(" ", new[] { u.FirstName, u.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))),
                    City = u.City,
                    Country = u.Country,
                    ProfilePictureUrl = u.ProfilePicturePath,
                    Initials = GetInitials(u.FirstName, u.LastName)
                })
                .ToListAsync();

            return new Dtos.PagedResult<AlumniCardDto>(items, total, page, pageSize);
        }

        private static string GetInitials(string? first, string? last)
        {
            var a = string.IsNullOrWhiteSpace(first) ? "" : first.Trim()[0].ToString();
            var b = string.IsNullOrWhiteSpace(last) ? "" : last.Trim()[0].ToString();
            var res = (a + b).ToUpper();
            return string.IsNullOrEmpty(res) ? "--" : res;
        }
    }
}
