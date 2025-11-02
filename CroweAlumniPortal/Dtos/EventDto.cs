using System.ComponentModel.DataAnnotations;

namespace CroweAlumniPortal.Dtos
{
    public class EventDto
    {
        public int? Id { get; set; }

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

        public bool? IsActive{ get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public int? LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; } = DateTime.Now;
    }
}
