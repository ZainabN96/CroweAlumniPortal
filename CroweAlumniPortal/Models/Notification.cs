namespace CroweAlumniPortal.Models
{
    public class Notification : AuditableEntity
    {
        public int Id { get; set; }
        public string Type { get; set; }         
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Url { get; set; }                    
        public bool IsGlobal { get; set; } = false;         
    }

    public class NotificationUser
    {
        public int Id { get; set; }
        public int NotificationId { get; set; }
        public Notification Notification { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadOn { get; set; }
    }
}
