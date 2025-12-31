namespace CroweAlumniPortal.Dtos
{
    public class AlumniReportRowDto
    {
        public int Sr { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string MobileNumber { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string Industry { get; set; } = "";
        public string Organization { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public DateTime? JoiningDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        public string MemberStatus { get; set; } = "";
        public int Id { get; set; }                // ✅ UI “View” ke liye (export me include nahi karenge)
    }
}
