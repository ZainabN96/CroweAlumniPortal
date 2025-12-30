// Helpers/EmailTemplates.cs
using System.Net;

namespace CroweAlumniPortal.Helper
{
    public static class EmailTemplates
    {
        // ====== Branding (edit for your portal) ======
        private const string BrandName = "Crowe Alumni Portal";
        private const string BrandSupport = "Alumni@hcctechfoundation.com";
        private const string WebUrl = "https://www.crowe.com/pk";  
        private const string BaseUrl = "https://alumni.crowe.pk/"; 
        private const string LogoUrl = "https://alumni.crowe.pk/assets/img/Crowe.png";

        // ====== Small helpers ======
        private static string H(string? x) => WebUtility.HtmlEncode(x ?? string.Empty);

        private static string Button(string text, string href) => $@"
            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"">
              <tr>
                <td style=""background:#005c97;border-radius:15px"">
                  <a href=""{H(href)}"" style=""display:inline-block;margin:10px 20px;color:#fff;
                     text-decoration:none;font-weight:600;font-family:Segoe UI,Arial,sans-serif"">
                     {H(text)}
                  </a>
                </td>
              </tr>
            </table>";

        private static string Wrap(string title, string bodyHtml) => $@"
            <!doctype html>
            <html>
            <head>
            <meta charset=""utf-8"">
            <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
            <title>{H(title)} · {BrandName}</title>
            </head>
            <body style=""margin:0;background:#f6f8fb"">
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background:#f6f8fb"">
                <tr>
                    <td align=""center"" style=""padding:24px 12px"">
                    <table role=""presentation"" width=""100%"" style=""max-width:640px;background:#ffffff;border-radius:14px;
                            box-shadow:0 2px 8px rgba(16,24,40,.06)"" cellspacing=""0"" cellpadding=""0"">
                        <tr>
                        <td style=""padding:20px 24px;border-bottom:1px solid #eef1f5"">
                            <table width=""100%"" role=""presentation"">
                            <tr>
                                <td style=""vertical-align:middle"">
                                <img src=""{H(LogoUrl)}"" alt=""{H(BrandName)}"" style=""height:50px"">
                                </td>
                                <td align=""right"" style=""font:600 14px Segoe UI,Arial,sans-serif;color:#101828"">{H(BrandName)}</td>
                            </tr>
                            </table>
                        </td>
                        </tr>

                        <tr>
                        <td style=""padding:24px 24px 8px;font:700 20px Segoe UI,Arial,sans-serif;color:#101828"">
                            {H(title)}
                        </td>
                        </tr>

                        <tr>
                        <td style=""padding:0 24px 24px;font:400 14px Segoe UI,Arial,sans-serif;color:#344054;line-height:1.6"">
                            {bodyHtml}
                        </td>
                        </tr>

                        <tr>
                        <td style=""padding:16px 24px;border-top:1px solid #eef1f5;font:400 12px Segoe UI,Arial,sans-serif;color:#667085"">
                            This email was sent by {H(BrandName)}. Need help? Contact <a href=""mailto:{H(BrandSupport)}""
                                style=""color:#0b5ed7;text-decoration:none"">{H(BrandSupport)}</a>.
                        </td>
                        </tr>
                    </table>

                    <div style=""padding:14px 8px;font:400 12px Segoe UI,Arial,sans-serif;color:#98a2b3"">
                        © {DateTime.UtcNow:yyyy} {H(BrandName)}. All rights reserved.
                    </div>
                    </td>
                </tr>
                </table>
            </body>
            </html>"
         ;

        // ====== Registration: Pending / Approved / Rejected ======
        public static string PendingSubject => "Alumni Registration Received — Pending Approval";
        public static string PendingBody(string first, string last)
        {
            var body = $@"
                <p>Dear {H(first)} {H(last)},</p>
                <p>Thank you for registering with the <b>{H(BrandName)}</b>.
                   Your application is currently under review. You will be notified once it is approved.</p>
                <p>You can check updates by signing in later.</p>
                {Button("Visit Our Website", WebUrl)}
                <p style=""margin-top:18px"">Regards,<br>{H(BrandName)} Team</p>";
            return Wrap("Registration received", body);
        }

        public static string ApprovedSubject => "Your Alumni Account is Approved";
        public static string ApprovedBody(string first, string last, string loginId, string? tempPassword = null)
        {
            var creds = string.IsNullOrWhiteSpace(tempPassword)
                ? $@"<p><b>Login ID:</b> {H(loginId)}</p>"
                : $@"<p><b>Login ID:</b> {H(loginId)}<br><b> Password:</b> {H(tempPassword)}</p>";

            var body = $@"
                <p>Dear {H(first)} {H(last)},</p>
                <p>Congratulations! Your alumni account has been approved.</p>
                {creds}
                <p>Please sign in and consider updating your profile.</p>
                {Button("Sign in ", $"{BaseUrl}/Home/Login")}
                <p style=""margin-top:18px"">Regards,<br>{H(BrandName)} Team</p>";
            return Wrap("Account approved", body);
        }

        public static string RejectedSubject => "Your Alumni Account Request";
        public static string RejectedBody(string first, string last, string reason)
        {
            var body = $@"
                <p>Dear {H(first)} {H(last)},</p>
                <p>Unfortunately, your alumni account request could not be approved.</p>
                <p><b>Reason:</b> {H(reason)}</p>
                <p>If you believe this was in error, reply to this email with supporting details.</p>
                <p style=""margin-top:18px"">Regards,<br>{H(BrandName)} Team</p>";
            return Wrap("Request not approved", body);
        }

        // ====== Post Moderation ======
        public static string PostSoftDeletedSubject(string postTitle) =>
            $"Your post was removed: {postTitle}";

        public static string PostSoftDeletedBody(string firstName, string postTitle, string adminName, long postId)
        {
            var body = $@"
                <p>Hi {H(firstName)},</p>
                <p>Your post <b>{H(postTitle ?? "")}</b> (ID: {postId}) has been
                   <b>removed by {H(adminName)}</b> for violating community guidelines or at the admin’s discretion.</p>
                <p>If you believe this was a mistake, you may reply to this email to request a review.</p>
                {Button("View Community Guidelines", $"{BaseUrl}/community-guidelines")}
                <hr style=""border:none;border-top:1px solid #eef1f5;margin:16px 0"">
                <p style=""color:#667085"">This is a system notification from the Alumni Portal.</p>";
            return Wrap("Post removed", body);
        }

        // ====== Events ======
        public static string EventCreatedSubject(string title) => $"New Event: {title}";
        public static string EventCreatedBody(string authorName, string title, string? description, DateTime? start, string detailsUrl)
        {
            var when = start.HasValue ? $"<p><b>Date & Time:</b> {H(start.Value.ToString("MMM dd, yyyy hh:mm tt"))}</p>" : "";
            var desc = string.IsNullOrWhiteSpace(description) ? "" : $"<p>{H(description)}</p>";

            var body = $@"
                <p>Hi,</p>
                <p><b>{H(authorName)}</b> added a new event: <b>{H(title)}</b>.</p>
                {when}
                {desc}
                {Button("View event", detailsUrl)}
                <p style=""margin-top:18px"">Regards,<br>{H(BrandName)} Team</p>";
            return Wrap("New event published", body);
        }

        public static string EventUpdatedSubject(string title) => $"Event Updated: {title}";
        public static string EventUpdatedBody(string title, string? changes, string detailsUrl)
        {
            var changeBlock = string.IsNullOrWhiteSpace(changes) ? "" : $"<p><b>What changed:</b> {H(changes)}</p>";
            var body = $@"
                <p>The event <b>{H(title)}</b> has been updated.</p>
                {changeBlock}
                {Button("View event", detailsUrl)}";
            return Wrap("Event updated", body);
        }

        public static string EventCancelledSubject(string title) => $"Event Cancelled: {title}";
        public static string EventCancelledBody(string title, string? reason)
        {
            var rsn = string.IsNullOrWhiteSpace(reason) ? "" : $"<p><b>Reason:</b> {H(reason)}</p>";
            var body = $@"
                <p>We regret to inform you that the event <b>{H(title)}</b> has been cancelled.</p>
                {rsn}
                <p>We apologize for any inconvenience.</p>";
            return Wrap("Event cancelled", body);
        }

        public static string EventReminderSubject(string title) => $"Reminder: {title} (starting soon)";
        public static string EventReminderBody(string title, DateTime start, string detailsUrl)
        {
            var body = $@"
                <p>This is a friendly reminder for <b>{H(title)}</b>.</p>
                <p><b>Date & Time:</b> {H(start.ToString("MMM dd, yyyy hh:mm tt"))}</p>
                {Button("Event details", detailsUrl)}";
            return Wrap("Event reminder", body);
        }

        // ====== Utility / Account ======
        public static string PasswordResetSubject => "Reset your password";
        public static string PasswordResetBody(string name, string resetUrl)
        {
            var body = $@"
                <p>Hi {H(name)},</p>
                <p>Click the button below to reset your password. This link will expire soon.</p>
                {Button("Reset password", resetUrl)}
                <p>If you didn’t request this, you can safely ignore this email.</p>";
            return Wrap("Password reset", body);
        }

        public static string VerifyEmailSubject => "Verify your email address";
        public static string VerifyEmailBody(string name, string verifyUrl)
        {
            var body = $@"
                <p>Hi {H(name)},</p>
                <p>Thanks for signing up! Please verify your email to activate your account.</p>
                {Button("Verify email", verifyUrl)}";
            return Wrap("Verify your email", body);
        }
     
        public static string NewMessageSubject(string senderName) =>
        $"New message from {senderName}";
        public static string NewMessageBody(string receiverName, string senderName, string? preview, string detailsUrl)
        {
            preview = string.IsNullOrWhiteSpace(preview) ? "Sent an attachment." : preview;

            var body = $@"
                <p>Hi {H(receiverName)},</p>
                <p><b>{H(senderName)}</b> sent you a new message.</p>
                
                {Button("Open Conversation", detailsUrl)}
                <p style=""margin-top:18px"">Regards,<br>{H(BrandName)} Team</p>";

            return Wrap("New message", body);   
        }

        // ====== Posts ======

        public static string PostCreatedSubject(string title) =>
            $"New post: {title}";

        public static string PostCreatedBody(string receiverName, string authorName, string title, string? bodyPreview, string detailsUrl)
        {
            var preview = string.IsNullOrWhiteSpace(bodyPreview) ? "" : $@"
        <div style=""margin:12px 0;padding:12px 14px;border:1px solid #eef1f5;border-radius:10px;background:#fbfcfe"">
            {H(bodyPreview.Length > 180 ? bodyPreview[..180] + "..." : bodyPreview)}
        </div>";

            var body = $@"
        <p>Hi {H(receiverName)},</p>
        <p><b>{H(authorName)}</b> added a new post: <b>{H(title ?? "Untitled")}</b>.</p>
        
        {Button("View post", detailsUrl)}
        <p style=""margin-top:18px"">Regards,<br>{H(BrandName)} Team</p>";

            return Wrap("New post published", body);
        }

        public static string PostLikedSubject(string likerName) =>
            $"{likerName} liked your post";

        public static string PostLikedBody(string ownerName, string likerName, string postTitle, string detailsUrl)
        {
            var body = $@"
        <p>Hi {H(ownerName)},</p>
        <p><b>{H(likerName)}</b> liked your post <b>{H(postTitle ?? "your post")}</b>.</p>
        {Button("View post", detailsUrl)}
        <p style=""margin-top:18px"">Regards,<br>{H(BrandName)} Team</p>";

            return Wrap("New like", body);
        }

        public static string PostCommentedSubject(string commenterName) =>
            $"{commenterName} commented on your post";

        public static string PostCommentedBody(string ownerName, string commenterName, string postTitle, string? commentPreview, string detailsUrl)
        {
            var preview = string.IsNullOrWhiteSpace(commentPreview) ? "" : $@"
        <div style=""margin:12px 0;padding:12px 14px;border:1px solid #eef1f5;border-radius:10px;background:#fbfcfe"">
            {H(commentPreview.Length > 180 ? commentPreview[..180] + "..." : commentPreview)}
        </div>";

            var body = $@"
        <p>Hi {H(ownerName)},</p>
        <p><b>{H(commenterName)}</b> commented on your post <b>{H(postTitle ?? "your post")}</b>.</p>
        
        {Button("View post", detailsUrl)}
        <p style=""margin-top:18px"">Regards,<br>{H(BrandName)} Team</p>";

            return Wrap("New comment", body);
        }

    }

}
