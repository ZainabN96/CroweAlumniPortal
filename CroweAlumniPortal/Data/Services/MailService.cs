using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Data.IServices;

namespace CroweAlumniPortal.Data.Services
{
    public class MailService : IMailService
    {
        public async Task<string> SendEmailAsync(MailRequestDto mailRequest)
        {
            var builder = new BodyBuilder
            {
                HtmlBody = mailRequest.Body,
                TextBody = mailRequest.Body
            };

            var message = new MimeMessage();
            string fromAddress = "Crowe@hcctechfoundation.com";
            string fromName = "Crowe Alumni Admin";

            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress("User", mailRequest.ToEmail));
            message.Subject = mailRequest.Subject;
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync("mail.hcctechfoundation.com", 587, SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(fromAddress, "HccTech*1_2121");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return "Success";
        }
    }
}
