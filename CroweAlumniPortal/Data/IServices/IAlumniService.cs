// Data/IServices/IAlumniService.cs
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Models;

namespace CroweAlumniPortal.Data.IServices
{
    public interface IAlumniService
    {
        Task<IEnumerable<User>> GetAllAlumniAsync();
        Task<Dtos.PagedResult<AlumniCardDto>> GetAlumniAsync(string? search, int page, int pageSize);
    }
}
