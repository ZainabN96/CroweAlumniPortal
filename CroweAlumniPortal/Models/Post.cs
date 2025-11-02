using System.ComponentModel.DataAnnotations;

namespace CroweAlumniPortal.Models
{
    public class Post : AuditableEntity
    {
        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(4000)]
        public string Body { get; set; }

        [StringLength(500)]
        public string? MediaPath { get; set; } 
        [StringLength(50)]
        public string? MediaType { get; set; }
    }
    public class PostLike : AuditableEntity
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public Post Post { get; set; }
    }

    public class PostComment : AuditableEntity
    {
        public int PostId { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(1000)]
        public string Body { get; set; }
        public Post Post { get; set; }
    }

}
