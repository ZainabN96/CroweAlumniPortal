using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Models;

namespace CroweAlumniPortal.Data.IServices
{
    public interface IEventService
    {
        Task<Event> CreateAsync(EventDto dto, int? createdByUserId = null);
    }
}
