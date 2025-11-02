using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IUnitOfWork uow;
    private readonly INotificationService Notification;
    private readonly IMailService mailService;

    public AdminUsersController(IUnitOfWork uow, INotificationService Notification, IMailService mailService)
    {
        this.uow = uow; 
        this.Notification = Notification;
        this.mailService = mailService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await uow.UserService.GetUsersByStatusAsync(UserApprovalStatus.Pending, page, pageSize);
        return Ok(result); // { items, total }
    }

    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> Approve(long id)
    {
        var user = await uow.UserService.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.ApprovalStatus = UserApprovalStatus.Approved;
        user.IsActive = true;
        user.ApprovedAt = DateTime.UtcNow;
        user.ApprovedBy = User?.Identity?.Name ?? "admin";

        await uow.SaveAsync();
      
        var subject = EmailTemplates.ApprovedSubject;
        var body = EmailTemplates.ApprovedBody(user.FirstName, user.LastName, user.LoginId, user.Password);
        await mailService.SendEmailAsync(new MailRequestDto { ToEmail = user.EmailAddress, Subject = subject, Body = body });

        return Ok(new { message = "User approved.", user.Id, user.IsActive, user.ApprovalStatus });
    }

    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> Reject(long id, [FromBody] RejectDto dto)
    {
        var user = await uow.UserService.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.ApprovalStatus = UserApprovalStatus.Rejected;
        user.IsActive = false;
        user.RejectionReason = dto?.Reason;

        await uow.SaveAsync();
        await Notification.NotifyUserRejectedAsync(user, dto?.Reason ?? "Not specified");

        return Ok(new { message = "User rejected.", user.Id, user.ApprovalStatus });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetOne(long id)
    {
        var user = await uow.UserService.GetByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.LoginId,
            user.UserType,
            user.OrganizationType,
            user.FirstName,
            user.LastName,
            user.EmailAddress,
            user.MobileNumber,
            user.Title,
            user.DOB,
            user.CNIC,
            user.MemberStatus,
            user.Qualification,
            user.BloodGroup,
            user.Address,
            user.ZIP,
            user.Country,
            user.City,
            user.LinkedIn,
            user.EmploymentStatus,
            user.Industry,
            user.EmployerOrganization,
            user.JobTitle,
            user.EmployerCountry,
            user.EmployerCity,
            user.EmployerLandline1,
            user.EmployerFaxNumber,
            user.EmployerAddress,
            user.StaffCode,
            user.LastPosition,
            user.Department,
            user.HistoryCity,
            user.JoiningDate,
            user.LeavingDate,
            user.ApprovalStatus,
            user.CreatedAt,
            user.ApprovedAt,
            user.ApprovedBy,
            user.RejectionReason,
            user.ProfilePicturePath
        });
    }

}
