using CroweAlumniPortal.Dtos;

namespace CroweAlumniPortal.Data.IServices
{
    public interface IMailService
    {
        Task<string> SendEmailAsync(MailRequestDto mailRequest);
    }
}
