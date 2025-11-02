using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Models;

namespace CroweAlumniPortal.Data.Services
{
    public class EventService: IEventService
    {
        private readonly ApplicationDbContext dc;

        public EventService(ApplicationDbContext dc)
        {
            this.dc = dc;
        }

        public async Task<Event> CreateAsync(EventDto dto, int? createdBy)
        {
            if (dto.EndDateTime <= dto.StartDateTime)
                throw new InvalidOperationException("End date must be after start date");

            var entity = new Event
            {
                Title = dto.Title.Trim(),
                Address = dto.Address.Trim(),
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                RegistrationLink = string.IsNullOrWhiteSpace(dto.RegistrationLink) ? null : dto.RegistrationLink,
                Description = dto.Description.Trim(),
                IsActive = true,
                CreatedOn = DateTime.Now,
                CreatedBy = createdBy,    
                LastUpdatedOn = DateTime.Now,
                LastUpdatedBy = createdBy
            };

            dc.Events.Add(entity);
            await dc.SaveChangesAsync();
            return entity;
        }
    }
}
