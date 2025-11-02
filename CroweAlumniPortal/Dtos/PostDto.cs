namespace CroweAlumniPortal.Dtos
{
    public class PostDto
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string? MediaPath { get; set; }
        public IFormFile? Media { get; set; }
        public string? MediaType { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public int? LastUpdatedBy { get; set; }
        public bool? IsActive { get; set; }
    }
    public class CommentDto
    {
        public int PostId { get; set; }
        public string Body { get; set; }
    }
    public sealed class UserMiniDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicturePath { get; set; }
    }

    public sealed class CommentMiniDto
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime CreatedOn { get; set; }
        public UserMiniDto Author { get; set; }
    }

    public sealed class PostWithMetaDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string MediaPath { get; set; }
        public string MediaType { get; set; }
        public DateTime CreatedOn { get; set; }
        public UserMiniDto Author { get; set; }
        public int LikeCount { get; set; }
        public List<CommentMiniDto> Comments { get; set; }
    }
}
