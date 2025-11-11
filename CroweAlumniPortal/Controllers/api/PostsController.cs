using AutoMapper;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Data.Services;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : Controller
    {
        private readonly IUnitOfWork uow;
        private readonly IFileService files;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        private readonly IMailService mailService;

        public PostsController(IUnitOfWork uow, IFileService files, IMapper mapper, INotificationService notificationService, IMailService mailService)
        {
            this.uow = uow;
            this.files = files;
            this.mapper = mapper;
            this.notificationService = notificationService;
            this.mailService = mailService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] PostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // current user id from session
            int? userId = null;
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrWhiteSpace(userIdStr) && int.TryParse(userIdStr, out var parsed))
                userId = parsed;

            // media
            if (dto.Media is { Length: > 0 })
            {
                if (dto.Media.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    dto.MediaType = "image";
                else if (dto.Media.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                    dto.MediaType = "video";
                else
                    dto.MediaType = "file";

                dto.MediaPath = await files.SaveFileAsync(dto.Media, "assets/img/uploads/posts");
            }

            var created = await uow.PostService.CreateAsync(dto, userId);

            // author fetch (may be null if not logged-in)
            var author = userId.HasValue ? await uow.UserService.Get(userId.Value) : null;
            var authorName = (author == null)
                ? "Someone"
                : $"{author.FirstName ?? ""} {author.LastName ?? ""}".Trim();

            // 🔔 Send live notification to all (except author)
            await notificationService.CreateForAllAsync(new Notification
            {
                Type = "post",
                Title = "New Post Added",
                Message = $"{authorName} added a new post: {dto.Title ?? "Untitled"}",
                Url = $"/Posts/Details/{created.Id}"
            }, exceptUserId: userId);

            return Ok(new
            {
                id = created.Id,
                title = created.Title,
                body = created.Body,
                mediaPath = created.MediaPath,
                mediaType = created.MediaType,
                createdOn = created.CreatedOn,
                author = (author == null) ? null : new
                {
                    author.FirstName,
                    author.LastName,
                    author.ProfilePicturePath
                }
            });
        }

        [HttpGet("latest")]
        public async Task<IActionResult> Latest([FromQuery] int take = 10)
        {
            try
            {
                int? uid = null;
                var s = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var parsed)) uid = parsed;

                var items = await uow.PostService.QueryLatestCompactAsync(take);

                if (uid != null)
                {
                    var withIsLiked = new List<object>();
                    foreach (dynamic p in (IEnumerable<object>)items)
                    {
                        int postId = p.id;
                        bool isLiked = await uow.PostService.HasUserLikedAsync(postId, uid.Value);
                        withIsLiked.Add(new
                        {
                            id = p.id,
                            title = p.title,
                            body = p.body,
                            mediaPath = p.mediaPath,
                            mediaType = p.mediaType,
                            createdOn = p.createdOn,
                            likeCount = p.likeCount,
                            isLiked,
                            author = p.author
                        });
                    }
                    return Ok(withIsLiked);
                }

                return Ok(items);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.ToString(), title: "Failed to load latest posts");
            }
        }

        [HttpGet("latsest")]
        public async Task<IActionResult> Latests([FromQuery] int take = 10)
        {
            int? uid = null;
            var s = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var parsed)) uid = parsed;

            var list = await uow.PostService.GetLatestAsync(take);

            var shaped = await Task.WhenAll(list.Select(async p =>
            {
                var author = p.CreatedBy.HasValue ? await uow.UserService.Get(p.CreatedBy.Value) : null;
                var likeCount = await uow.PostService.GetLikeCountAsync(p.Id);
                var isLiked = (uid != null) && await uow.PostService.HasUserLikedAsync(p.Id, uid.Value);

                return new
                {
                    id = p.Id,
                    title = p.Title,
                    body = p.Body,
                    mediaPath = p.MediaPath,
                    mediaType = p.MediaType,
                    createdOn = p.CreatedOn,
                    likeCount,
                    isLiked,
                    author = author == null ? null : new
                    {
                        firstName = author.FirstName,
                        lastName = author.LastName,
                        profilePicturePath = author.ProfilePicturePath
                    }
                };
            }));

            return Ok(shaped);
        }

        [HttpPost("{id:int}/like/toggle")]
        public async Task<IActionResult> ToggleLike(int id)
        {
            int? uid = null;
            var s = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var parsed)) uid = parsed;
            if (uid == null) return Unauthorized();

            var isLiked = await uow.PostService.HasUserLikedAsync(id, uid.Value);
            if (isLiked)
            {
                await uow.PostService.UnlikeAsync(id, uid.Value);
            }
            else
            {
                await uow.PostService.LikeAsync(id, uid.Value);

                var ownerId = await uow.PostService.GetOwnerIdAsync(id);

                if (ownerId.HasValue)
                {
                    var liker = await uow.UserService.Get(uid.Value);
                    var post = await uow.PostService.GetByIdAsync(id);

                    var likerName = ((liker?.FirstName ?? "") + " " + (liker?.LastName ?? "")).Trim();
                    var postTitle = string.IsNullOrWhiteSpace(post?.Title) ? "your post" : $"\"{post!.Title}\"";

                    await notificationService.CreateForUserAsync(new Notification
                    {
                        Type = "like",
                        Title = "New like on your post",
                        Message = $"{likerName} liked {postTitle}.",
                        Url = $"/Posts/Details/{id}"
                    }, ownerId.Value);
                }
            }

            var count = await uow.PostService.GetLikeCountAsync(id);
            return Ok(new { isLiked = !isLiked, count });
        }

        [HttpPost("{postId}/like")]
        public async Task<IActionResult> Like(int postId)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            await uow.PostService.LikeAsync(postId, userId);
            return Ok();
        }

        [HttpPost("{postId}/unlike")]
        public async Task<IActionResult> Unlike(int postId)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            await uow.PostService.UnlikeAsync(postId, userId);

            return Ok();
        }

        [HttpPost("{postId}/comment")]
        public async Task<IActionResult> Comment(int postId, [FromBody] CommentDto dto)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            dto.PostId = postId;
            var comment = await uow.PostService.AddCommentAsync(dto, userId);
            var ownerId = await uow.PostService.GetOwnerIdAsync(postId);

            if (ownerId.HasValue)
            {
                var liker = await uow.UserService.Get(userId);
                var post = await uow.PostService.GetByIdAsync(postId);

                var likerName = ((liker?.FirstName ?? "") + " " + (liker?.LastName ?? "")).Trim();
                var postTitle = string.IsNullOrWhiteSpace(post?.Title) ? "your post" : $"\"{post!.Title}\"";

                await notificationService.CreateForUserAsync(new Notification
                {
                    Type = "comment",
                    Title = "New comment on your post",
                    Message = $"{likerName} has commented on {postTitle}.",
                    Url = $"/Dashboard/Dashboard#post-{postId}"
                }, ownerId.Value);
            }

            return Ok(comment);
        }


        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await uow.PostService.GetCommentsAsync(postId);

            var result = new List<object>();
            foreach (var c in comments)
            {
                var user = c.CreatedBy.HasValue
                    ? await uow.UserService.Get(c.CreatedBy.Value)
                    : null;

                result.Add(new
                {
                    c.Id,
                    c.Body,
                    c.CreatedOn,
                    Author = user == null ? null : new
                    {
                        user.FirstName,
                        user.LastName,
                        user.ProfilePicturePath
                    }
                });
            }

            return Ok(result);
        }

        [HttpPost("{id:int}/soft-delete")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            try
            {
                string adminName = "Admin";
                int? adminId = null;

                var userIdStr = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrWhiteSpace(userIdStr) && int.TryParse(userIdStr, out var parsedId))
                {
                    adminId = parsedId;
                    var adminUser = await uow.UserService.Get(parsedId);
                    if (adminUser != null)
                        adminName = $"{adminUser.FirstName} {adminUser.LastName}".Trim();
                }

                var post = await uow.PostService.GetByIdAsync(id);
                if (post == null) return NotFound();

                var author = await uow.UserService.Get((int)post.CreatedBy);
                if (author == null) return NotFound();

                await uow.PostService.SoftDeleteAsync(id, adminName);
                await uow.SaveAsync();

                try
                {
                    await notificationService.NotifyPostSoftDeletedAsync(author, post, adminId ?? 0, adminName);
                }
                catch
                {
                    // log later
                }

                try
                {
                    var subject = $"Your post was removed by {adminName}";
                    var body = EmailTemplates.PostSoftDeletedBody(
                        author.FirstName,
                        post.Title,
                        adminName,
                        post.Id);

                    await mailService.SendEmailAsync(new MailRequestDto
                    {
                        ToEmail = author.EmailAddress,
                        Subject = subject,
                        Body = body
                    });
                }
                catch
                {
                    // log later
                }

                return Ok(new { message = "Post deleted." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }


        [HttpPost("{id:long}/restore")]
        public async Task<IActionResult> Restore(long id)
        {
            try
            {
                await uow.PostService.RestoreAsync(id);
                return Ok(new { message = "Post restored." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id:int}/like/state")]
        public async Task<IActionResult> LikeState(int id)
        {
            int? uid = null;
            var s = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var parsed)) uid = parsed;

            var likeCount = await uow.PostService.GetLikeCountAsync(id);
            var isLiked = uid.HasValue && await uow.PostService.HasUserLikedAsync(id, uid.Value);
            return Ok(new { likeCount, isLiked });
        }
    }
}
