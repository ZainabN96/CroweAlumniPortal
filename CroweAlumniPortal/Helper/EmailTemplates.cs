// Helpers/EmailTemplates.cs
namespace CroweAlumniPortal.Helper
{
    public static class EmailTemplates
    {
        public static string PendingSubject => "Alumni Registration Received – Pending Approval";
        public static string PendingBody(string first, string last) => $@"
            Dear {first} {last},<BR><BR>

            Thank you for registering with the Crowe Alumni Portal.<BR>
            Your application is currently under review. You will be notified once it is approved.<BR><BR>

            Regards,<BR>
            Crowe Alumni Team
        ";

        public static string ApprovedSubject => "Your Alumni Account is Approved";
        public static string ApprovedBody(string first, string last, string loginId, string? tempPassword = null) => $@"
            Dear {first} {last},<BR>
            
            Congratulations! Your alumni account has been approved.
            <BR>
            Your Login credentials are:
            <BR>
            Login ID: {loginId}
            {(string.IsNullOrWhiteSpace(tempPassword) ? "" : $"Password: {tempPassword}\n")}
            <BR>
            You can now sign in to the portal.
            <BR>
            Regards,<BR>
            Crowe Alumni Team
        ";

        public static string RejectedSubject => "Your Alumni Account Request";
        public static string RejectedBody(string first, string last, string reason) => $@"
            Dear {first} {last},<BR> <BR>

            Unfortunately, your alumni account request could not be approved.<BR> <BR>
            Reason: {reason}<BR>

            If you believe this was in error, please reply to this email.<BR>

            Regards,<BR>
            Crowe Alumni Team
        ";

        public static string PostSoftDeletedBody(string firstName, string postTitle, string adminName, long postId) => $@"
            <div style='font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#222'>
              <p>Hi {System.Net.WebUtility.HtmlEncode(firstName)},</p>
              <p>Your post <strong>{System.Net.WebUtility.HtmlEncode(postTitle ?? "Untitled")}</strong> (ID: {postId})
                 has been <strong>removed by {System.Net.WebUtility.HtmlEncode(adminName)}</strong> for violating our community guidelines
                 or at the admin’s discretion.</p>
              <p>If you believe this was a mistake, you can reply to this email to request a review.</p>
              <hr style='border:none;border-top:1px solid #eee;margin:16px 0' />
              <p style='color:#666'>This is a system notification from the Alumni Portal.</p>
            </div>
        ";
    }
}
