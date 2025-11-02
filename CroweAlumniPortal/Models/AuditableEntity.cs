namespace CroweAlumniPortal.Models
{
    public class AuditableEntity : BaseEntity
    {
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
