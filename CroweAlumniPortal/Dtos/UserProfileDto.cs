namespace CroweAlumniPortal.Dtos
{
    public class UserProfileDto
    {
        public string FullName => FirstName + " " + LastName;

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public string Qualification { get; set; }

        public string MemberStatus { get; set; }

        public string OrganizationType { get; set; }

        public string ProfilePicturePath { get; set; }
    }

}
