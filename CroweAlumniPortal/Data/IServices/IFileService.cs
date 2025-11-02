//using CloudinaryDotNet.Actions;

namespace CroweAlumniPortal.Data.IServices
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        
        bool DeleteFile(string relativePath);
    }
}
