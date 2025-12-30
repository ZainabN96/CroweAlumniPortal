using System.ComponentModel.DataAnnotations;

namespace CroweAlumniPortal.Models
{
    public class User : BaseEntity
    {
        // Profile
        [Required]
        public string Title { get; set; }
        public string? ProfilePicturePath { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateTime DOB { get; set; }
        [Required]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNIC must be 13 digits without dashes")]
        public string CNIC { get; set; }
        public string? MemberStatus { get; set; }

        [Required]
        public string Qualification { get; set; }

        public string? BloodGroup { get; set; }

        // Home Address
        [Required]
        public string Address { get; set; }
        public string? ZIP { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string MobileNumber { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        public string? LandLine1 { get; set; }

        public string? LandLine2 { get; set; }

        public string? FaxNumber { get; set; }

        public string? LinkedIn { get; set; }

        // User Type & Organization
        [Required]
        public string UserType { get; set; } // "Admin" or "Alumni"

        public string? OrganizationType { get; set; } // Crowe, HCC, HTF

        // Login Details
        [Required]
        public string LoginId { get; set; }

        [Required]
        public string EmploymentStatus { get; set; }

        [Required]
        public string Password { get; set; }

        public string? Question { get; set; }

        public string? SecretAnswer { get; set; }

        // Current Employer
        public string? Industry { get; set; }
        public string? EmployerOrganization { get; set; }
        public string? JobTitle { get; set; }
        public string? EmployerCountry { get; set; }
        public string? EmployerCity { get; set; }
        public string? EmployerLandline1 { get; set; }
        public string? EmployerFaxNumber { get; set; }
        public string? EmployerAddress { get; set; }

        // Crowe History
        public string? StaffCode { get; set; }

        [Required]
        public string? LastPosition { get; set; }

        [Required]
        public string? Department { get; set; }

        [Required]
        public string? HistoryCity { get; set; }
        [Required]
        public DateTime JoiningDate { get; set; }

        [Required]
        public DateTime LeavingDate { get; set; }

        [Required]
        public bool AgreePrivacy { get; set; }
        public UserApprovalStatus ApprovalStatus { get; set; } = UserApprovalStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectionReason { get; set; }

    }
    public enum UserApprovalStatus
    {
        [Display(Name = "Pending")]
        Pending = 0,

        [Display(Name = "Approved")]
        Approved = 1,

        [Display(Name = "Rejected")]
        Rejected = 2
    }

}