using System.ComponentModel.DataAnnotations;

namespace CroweAlumniPortal.Models
{
    public class Event: AuditableEntity
    {
        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(300)]
        public string Address { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Url]
        public string? RegistrationLink { get; set; }

        [Required, StringLength(2000)]
        public string Description { get; set; }

    }
}
