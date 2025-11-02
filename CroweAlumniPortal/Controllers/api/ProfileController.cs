using AutoMapper;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Data.Services;
using CroweAlumniPortal.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : Controller
    {
        private readonly IUnitOfWork uow;
        private readonly IFileService fileService;
        private readonly IMapper mapper;

        public ProfileController(IUnitOfWork uow, IFileService fileService, IMapper mapper)
        {
            this.uow = uow;
            this.fileService = fileService;
            this.mapper = mapper;
        }
        // helper: current user id from session
        private int? CurrentUserId =>
                int.TryParse(HttpContext.Session.GetString("UserId"), out var id) ? id : (int?)null;

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var uid = CurrentUserId;
            if (uid == null) return Unauthorized();

            var user = await uow.UserService.Get(uid.Value);
            if (user == null) return NotFound();

            var dto = mapper.Map<ProfileDto>(user);
            return Ok(dto);
        }

        [HttpPut("basic")]
        public async Task<IActionResult> UpdateBasic([FromBody] ProfileDto dto)
        {
            var uid = CurrentUserId;
            if (uid == null) return Unauthorized();

            var user = await uow.UserService.Get(uid.Value);
            if (user == null) return NotFound();

            user.FirstName = dto.FirstName?.Trim();
            user.LastName = dto.LastName?.Trim();
            user.DOB = dto.DOB ?? user.DOB;
            user.City = dto.CurrentCity?.Trim();
            //user.Gender = dto.Gender;
            user.Qualification = user.Qualification; // keep old if needed
            user.LastUpdatedOn = DateTime.UtcNow;
            user.LastUpdatedBy = uid;

            await uow.SaveAsync();
            return Ok();
        }

        [HttpPut("contact")]
        public async Task<IActionResult> UpdateContact([FromBody] ProfileDto dto)
        {
            var uid = CurrentUserId;
            if (uid == null) return Unauthorized();

            var user = await uow.UserService.Get(uid.Value);
            if (user == null) return NotFound();

            user.EmailAddress = dto.EmailAddress?.Trim();
            user.MobileNumber = dto.Phone?.Trim();
            user.LinkedIn = dto.LinkedIn?.Trim();
            user.Address = dto.Address?.Trim();
            user.City = dto.City?.Trim();
            user.ZIP = dto.ZIP?.Trim();
            user.LastUpdatedOn = DateTime.UtcNow;
            user.LastUpdatedBy = uid;

            await uow.SaveAsync();
            return Ok();
        }

        [HttpPut("other")]
        public async Task<IActionResult> UpdateOther([FromBody] ProfileDto dto)
        {
            var uid = CurrentUserId;
            if (uid == null) return Unauthorized();

            var user = await uow.UserService.Get(uid.Value);
            if (user == null) return NotFound();

            user.JobTitle = dto.Designation?.Trim();
            // Skills ko agar separate table chahiye to service me handle karen.
            user.LastUpdatedOn = DateTime.UtcNow;
            user.LastUpdatedBy = uid;
            await uow.SaveAsync();
            return Ok();
        }

        [HttpPut("privacy")]
        public async Task<IActionResult> UpdatePrivacy([FromBody] ProfileDto dto)
        {
            var uid = CurrentUserId;
            if (uid == null) return Unauthorized();

            var user = await uow.UserService.Get(uid.Value);
            if (user == null) return NotFound();

            // map to your privacy columns; yahan simple strings store kiye.
            user.LastUpdatedOn = DateTime.UtcNow;
            user.LastUpdatedBy = uid;
            await uow.SaveAsync();
            return Ok();
        }

        [HttpPost("photo")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file)
        {
            var uid = CurrentUserId;
            if (uid == null) return Unauthorized();
            if (file == null || file.Length == 0) return BadRequest("File missing.");

            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowed.Contains(file.ContentType)) return BadRequest("Only images allowed.");

            //var path = await files.SaveFileAsync(file, "assets/img/uploads/profiles"); // returns relative path "uploads/profiles/.."

            var path = await fileService.SaveFileAsync(file, "assets/img/uploads/profiles");
            var user = await uow.UserService.Get(uid.Value);
            user.ProfilePicturePath = path;
            //user.ProfilePicturePath = "/" + path.Replace("\\", "/");
            user.LastUpdatedOn = DateTime.UtcNow;
            user.LastUpdatedBy = uid;
            await uow.SaveAsync();

            return Ok(new PhotoUploadResultDto { Url = user.ProfilePicturePath });

        }
        
    }
}
