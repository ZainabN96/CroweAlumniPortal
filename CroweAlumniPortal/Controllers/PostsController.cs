using System.Security.Claims;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.Mvc;

[Route("Posts")]
public class PostsController : Controller
{
    private readonly IUnitOfWork uow;
    public PostsController(IUnitOfWork uow) => this.uow = uow;

    private async Task HydrateSessionFromClaimsAsync()
    {
        // already set? skip
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId")))
            return;

        // not logged-in? skip
        if (!(User?.Identity?.IsAuthenticated ?? false))
            return;

        // try common id claims
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(idStr, out var uid))
            return;

        var user = await uow.UserService.Get(uid);
        if (user == null) return;

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserType", user.UserType ?? "");
    }

    private bool IsAdminFromSessionOrClaims(string LoginuserType)
    {
        // claims role
        if (LoginuserType == "Admin") return true;

        return false;
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        string LoginuserType = "";
        int? adminId = null;

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrWhiteSpace(userIdStr) && int.TryParse(userIdStr, out var parsedId))
        {
            adminId = parsedId;
            var adminUser = await uow.UserService.Get(parsedId);
            if (adminUser != null)
                LoginuserType = $"{adminUser.UserType}".Trim();
        }
        
        var post = await uow.PostService.GetByIdAsync(id);

        if (post == null || post.IsDeleted) return NotFound();

        var author = post.CreatedBy.HasValue
            ? await uow.UserService.Get(post.CreatedBy.Value)
            : null;

        ViewBag.AuthorFirstName = author?.FirstName;
        ViewBag.AuthorLastName = author?.LastName;
        ViewBag.AuthorPic = author?.ProfilePicturePath;

        // admin check
        ViewBag.IsAdmin = IsAdminFromSessionOrClaims(LoginuserType);

        return View(post);
    }
}