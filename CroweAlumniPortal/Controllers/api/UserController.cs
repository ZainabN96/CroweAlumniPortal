using AutoMapper;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Data.Services;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Errors;
using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUnitOfWork uow;
        private readonly IMapper mapper;
        private readonly IFileService fileService;
        private readonly INotificationService notificationService;
        private readonly IMailService mailService;
        public UserController(IUnitOfWork uow, IMapper mapper, IFileService fileService, INotificationService notificationService, IMailService mailService)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.fileService = fileService;
            this.notificationService = notificationService;
            this.mailService = mailService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto loginReqDTO)
        {
            var user = await uow.UserService.Authenticate(loginReqDTO.LoginId, loginReqDTO.Password);
            APIError apiError = new APIError();
            if (user == null)
            {
                apiError.ErrorCode = Unauthorized().StatusCode;
                apiError.ErrorMessage = "Login Id or password Invalid";
                return Unauthorized(apiError);
            }
            HttpContext.Session.SetString("UserId", user.Id.ToString());

            var loginRes = new LoginRepDto();
            loginRes.Id = user.Id;
            loginRes.LoginId = user.LoginId;
            loginRes.FirstName = user.FirstName;
            loginRes.LastName = user.LastName;
            loginRes.UserType = user.UserType;

            return Ok(loginRes);
        }
        public async Task<User?> ValidateLoginAsync(string email, string password)
        {
            var user = await uow.UserService.GetByEmailAsync(email);
            if (user == null) return null;

            if (user.ApprovalStatus != UserApprovalStatus.Approved || !user.IsActive)
                throw new UnauthorizedAccessException("Your account is pending approval. Please wait for admin.");

            return user;
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok("Logged out successfully.");
        }
        
        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] UserDto dto) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? picPath = null;
            if (dto.ProfilePhoto is { Length: > 0 })
            {
                var okTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
                if (!okTypes.Contains(dto.ProfilePhoto.ContentType))
                    return BadRequest("Only JPG/PNG/WebP/GIF allowed for profile photo.");

                picPath = await fileService.SaveFileAsync(dto.ProfilePhoto, "assets/img/uploads/profiles");
                dto.ProfilePicturePath = picPath;  
            }

            var user = mapper.Map<User>(dto);
            var created = await uow.UserService.RegisterAsync(user);

            await mailService.SendEmailAsync(new MailRequestDto
            {
                ToEmail = created.EmailAddress,
                Subject = Helper.EmailTemplates.PendingSubject,
                Body = Helper.EmailTemplates.PendingBody(created.FirstName, created.LastName)
            });

            await notificationService.NotifyAdminsNewUserAsync(created);

            return Ok(new
            {
                created.Id,
                created.LoginId,
                created.UserType,
                created.OrganizationType,
                created.EmailAddress,
                created.FirstName,
                created.LastName,
                created.ProfilePicturePath
            });
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var entry = await uow.UserService.Get(id);

            if (entry == null)
            {
                APIError apiError = new APIError();
                apiError.ErrorCode = NoContent().StatusCode;
                apiError.ErrorMessage = "User not found";
                return BadRequest(apiError);
            }

            return Ok(entry);
        }
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromQuery] string? status = "Approved")
        {
            UserApprovalStatus? s = null;
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<UserApprovalStatus>(status, ignoreCase: true, out var parsed))
            {
                s = parsed;
            }

            var count = await uow.UserService.CountAsync(s);
            return Ok(new { count });
        }
    }
}
