using System.ComponentModel.DataAnnotations;

namespace CroweAlumniPortal.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserType { get; set; }
        public string OrganizationType { get; set; }
        [Required] public string Title { get; set; }
        public string? ProfilePicturePath { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] 
        public string LastName { get; set; }
        [Required]
        public DateTime DOB { get; set; }
        [Required]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNIC must be 13 digits without dashes")]
        public string CNIC { get; set; }
        [Required] 
        public string MemberStatus { get; set; }
        [Required] 
        public string Qualification { get; set; }
        public string BloodGroup { get; set; }
        [Required] public string Address { get; set; }
        public string ZIP { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string MobileNumber { get; set; }
        [Required] 
        public string EmailAddress { get; set; }
        public string LandLine1 { get; set; }
        public string LandLine2 { get; set; }
        public string FaxNumber { get; set; }
        public string LinkedIn { get; set; }
      
        public string? LoginId { get; set; }
        [Required] 
        public string EmploymentStatus { get; set; }
        [Required] 
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Required] 
        public string ConfirmPassword { get; set; }

        public string Question { get; set; }
        public string SecretAnswer { get; set; }
        [Required] 
        public string Industry { get; set; }
        [Required] 
        public string EmployerOrganization { get; set; }
        public string JobTitle { get; set; }
        public string EmployerCountry { get; set; }
        public string EmployerCity { get; set; }
        public string EmployerLandline1 { get; set; }
        public string EmployerFaxNumber { get; set; }
        [Required] 
        public string EmployerAddress { get; set; }
        public string StaffCode { get; set; }
        [Required] 
        public string LastPosition { get; set; }
        [Required] 
        public string Department { get; set; }
        [Required] 
        public string HistoryCity { get; set; }
        public DateTime? JoiningDate { get; set; }
        [Required] 
        public DateTime LeavingDate { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public bool AgreePrivacy { get; set; }
        public DateTime LastUpdatedOn { get; set; } = DateTime.Now;
        public int LastUpdatedBy { get; set; }
    }
    public class ProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DOB { get; set; }
        public string CurrentCity { get; set; }
        public string Gender { get; set; }
        public string About { get; set; }
        public string? ProfilePicturePath { get; set; }
        public string EmailAddress { get; set; }
        public string AltEmail { get; set; }
        public string PhoneCode { get; set; }  
        public string Phone { get; set; }
        public string LinkedIn { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZIP { get; set; }
        public string Designation { get; set; }
        public List<string> Skills { get; set; } = new();
        public string EmailVisibility { get; set; } 
        public string PhoneVisibility { get; set; }
        public bool InstituteMails { get; set; }
        public bool SystemMails { get; set; }
    }
    public class PhotoUploadResultDto
    {
        public string Url { get; set; }
    }

}
