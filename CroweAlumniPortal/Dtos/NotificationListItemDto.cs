namespace CroweAlumniPortal.Dtos
{
    public class NotificationListItemDto
    {
        public int NotificationUserId { get; set; }   // NotificationUser.Id
        public int NotificationId { get; set; }

        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? Url { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }      // from Notification / AuditableEntity
        public DateTime? ReadOn { get; set; }
    }
}
