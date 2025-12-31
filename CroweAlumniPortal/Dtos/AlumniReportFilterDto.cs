namespace CroweAlumniPortal.Dtos
{
    public class AlumniReportFilterDto
    {
        public string? Search { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Industry { get; set; }
        public int? JoinYear { get; set; }
        public int? LeaveYear { get; set; }
    }
    //public class AlumniReportFilterDTo
    //{
    //    public int Page { get; set; } = 1;
    //    public int PageSize { get; set; } = 20;
    //    public string? Name { get; set; }
    //    public string? Email { get; set; }
    //    public string? City { get; set; }
    //    public string? Status { get; set; }
    //    public DateTime? From { get; set; }
    //    public DateTime? To { get; set; }
    //}

}
