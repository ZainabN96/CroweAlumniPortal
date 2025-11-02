namespace CroweAlumniPortal.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public bool IsGroup { get; set; } = false;
        public string? Title { get; set; }   
        public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
    public class ConversationMember
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime? LastSeenOn { get; set; }  
    }
    public class Message : AuditableEntity
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public int SenderId { get; set; }        
        public string Body { get; set; } = "";
        public string? MediaPath { get; set; }
        public string? MediaType { get; set; }     
        public bool IsDeleted { get; set; } = false;
    }
}
