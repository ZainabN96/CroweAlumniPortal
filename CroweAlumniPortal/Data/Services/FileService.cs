using CroweAlumniPortal.Data.IServices;

namespace CroweAlumniPortal.Data.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }
        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Empty file");

            var sub = folder.TrimStart('/', '\\'); 
            var root = _env.WebRootPath;           
            var dir = Path.Combine(root, sub);

            Directory.CreateDirectory(dir);

            var ext = Path.GetExtension(file.FileName);
            var name = $"{Guid.NewGuid():N}{ext}";
            var physicalPath = Path.Combine(dir, name);

            using (var fs = new FileStream(physicalPath, FileMode.Create))
                await file.CopyToAsync(fs);

            var url = "/" + Path.Combine(sub, name).Replace("\\", "/");
            return url;
            
        }
        public bool DeleteFile(string relativePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativePath)) return false;

                var webroot = _env.WebRootPath ?? throw new InvalidOperationException("WebRootPath not available.");
                var rel = relativePath.TrimStart('~').TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
                var full = Path.Combine(webroot, rel);

                if (File.Exists(full))
                {
                    File.Delete(full);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteFile failed for {Path}", relativePath);
                return false;
            }
        }
    }
}
