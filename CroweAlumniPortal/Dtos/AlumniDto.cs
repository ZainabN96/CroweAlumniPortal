namespace CroweAlumniPortal.Dtos
{
    public class AlumniDto
    {
    }
    
    public class AlumniCardDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Initials { get; set; }
    }

    public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
}
