using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Dtos;

namespace CroweAlumniPortal.Data.Services
{
    public class EmailSenderAdapter : IEmailSender
    {
        private readonly IMailService _mail;
        public EmailSenderAdapter(IMailService mail) { _mail = mail; }

        public Task SendAsync(string to, string subject, string body)
            => _mail.SendEmailAsync(new MailRequestDto { ToEmail = to, Subject = subject, Body = body, Attachments = new() });
    }
}
