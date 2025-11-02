using CroweAlumniPortal.Data;
using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Models;
using Microsoft.EntityFrameworkCore;

public class DbAdminDirectory : IAdminDirectory
{
    private readonly ApplicationDbContext _dc;
    public DbAdminDirectory(ApplicationDbContext dc) { _dc = dc; }

    public Task<List<string>> GetAllAdminEmailsAsync() =>
        _dc.Users
           .Where(u => u.UserType == "Admin" && !string.IsNullOrEmpty(u.EmailAddress))
           .Select(u => u.EmailAddress)
           .Distinct()
           .ToListAsync();
    public Task<List<int>> GetAllAdminIdsAsync() => 
        _dc.Users
        .Where(u => u.UserType == "Admin")
        .Select(u => u.Id)
        .Distinct()
        .ToListAsync();
}
