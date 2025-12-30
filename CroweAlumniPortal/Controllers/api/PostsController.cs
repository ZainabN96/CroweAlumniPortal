using AutoMapper;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Data.Services;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Asn1.X509;

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
        private const string BaseUrl = "https://alumni.crowe.pk/";

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
            // ✉ Email to all only Approved users
            var allUsers = await uow.UserService.ListAllAsync(); // create this if missing
            var recipients = allUsers
                .Where(u => u.ApprovalStatus == UserApprovalStatus.Approved)
                .Where(u => !string.IsNullOrWhiteSpace(u.EmailAddress))
                .Where(u => !userId.HasValue || u.Id != userId.Value)
                .ToList();

            var postUrl = $"{BaseUrl}/Posts/Details/{created.Id}"; 
            foreach (var r in recipients)
            {
                try
                {
                    var receiverName = $"{r.FirstName} {r.LastName}".Trim();
                    var subject = EmailTemplates.PostCreatedSubject(created.Title ?? "Untitled");
                    var body = EmailTemplates.PostCreatedBody(
                        receiverName,
                        authorName,
                        created.Title ?? "Untitled",
                        created.Body,
                        postUrl
                    );

                    await mailService.SendEmailAsync(new MailRequestDto
                    {
                        ToEmail = r.EmailAddress,
                        Subject = subject,
                        Body = body
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending event created email to {r.EmailAddress}: {ex.Message}");
                }
            }

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
                    try
                    {
                        var owner = await uow.UserService.Get(ownerId.Value);
                        if (owner != null && !string.IsNullOrWhiteSpace(owner.EmailAddress))
                        {
                            var ownerName = $"{owner.FirstName} {owner.LastName}".Trim();
                            var subject = EmailTemplates.PostLikedSubject(likerName);
                            var body = EmailTemplates.PostLikedBody(
                                ownerName,
                                likerName,
                                post?.Title ?? "your post",
                                $"{BaseUrl}/Posts/Details/{id}"
                            );

                            await mailService.SendEmailAsync(new MailRequestDto
                            {
                                ToEmail = owner.EmailAddress,
                                Subject = subject,
                                Body = body
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending event created email to onwer: {ex.Message}");
                    }

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

        /*[HttpPost("{postId}/comment")]
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
                // ✉ Email to post owner on COMMENT
                try
                {
                    var owner = await uow.UserService.Get(ownerId.Value);
                    if (owner != null && !string.IsNullOrWhiteSpace(owner.EmailAddress))
                    {
                        var ownerName = $"{owner.FirstName} {owner.LastName}".Trim();
                        var subject = EmailTemplates.PostCommentedSubject(likerName); // likerName is commenter here
                        var body = EmailTemplates.PostCommentedBody(
                            ownerName,
                            likerName,
                            post?.Title ?? "your post",
                            dto?.Body,
                            $"{BaseUrl}/Posts/Details/{postId}"
                        );

                        await mailService.SendEmailAsync(new MailRequestDto
                        {
                            ToEmail = owner.EmailAddress,
                            Subject = subject,
                            Body = body
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending event created email to owner: {ex.Message}");
                }
            }

            return Ok(comment);
        }
*/
        /*[HttpPost("{postId}/comment")]
        public async Task<IActionResult> Comment(int postId, [FromBody] CommentDto dto)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Body))
                return BadRequest("Comment is required.");

            dto.PostId = postId;

            // Ensure your service sets CreatedOn = DateTime.UtcNow
            var saved = await uow.PostService.AddCommentAsync(dto, userId);

            // Post owner
            var ownerId = await uow.PostService.GetOwnerIdAsync(postId);

            // Comment author
            var author = await uow.UserService.Get(userId);

            // Post (for title)
            var post = await uow.PostService.GetByIdAsync(postId);

            // Notification + Email to owner (only if owner is different person)
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
                // ✉ Email to post owner on COMMENT
                try
                {
                    var owner = await uow.UserService.Get(ownerId.Value);
                    if (owner != null && !string.IsNullOrWhiteSpace(owner.EmailAddress))
                    {
                        var ownerName = $"{owner.FirstName} {owner.LastName}".Trim();
                        var subject = EmailTemplates.PostCommentedSubject(likerName); // likerName is commenter here
                        var body = EmailTemplates.PostCommentedBody(
                            ownerName,
                            likerName,
                            post?.Title ?? "your post",
                            dto?.Body,
                            $"{BaseUrl}/Posts/Details/{postId}"
                        );

                        await mailService.SendEmailAsync(new MailRequestDto
                        {
                            ToEmail = owner.EmailAddress,
                            Subject = subject,
                            Body = body
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending post commented email to onwer: {ex.Message}");
                }
            }

            // ✅ Return same shape your UI expects
            return Ok(new
            {
                id = saved.Id,
                body = saved.Body,
                createdOn = saved.CreatedOn, // should be UTC Now
                author = author == null ? null : new
                {
                    firstName = author.FirstName,
                    lastName = author.LastName,
                    profilePicturePath = author.ProfilePicturePath
                }
            });
        }
*/
        [HttpPost("{postId}/comment")]
        public async Task<IActionResult> Comment(int postId, [FromBody] CommentDto dto)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            dto.PostId = postId;
            var comment = await uow.PostService.AddCommentAsync(dto, userId);
            var ownerId = await uow.PostService.GetOwnerIdAsync(postId);
            var author = await uow.UserService.Get(userId);
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
                // ✉ Email to post owner on COMMENT
                try
                {
                    var owner = await uow.UserService.Get(ownerId.Value);
                    if (owner != null && !string.IsNullOrWhiteSpace(owner.EmailAddress))
                    {
                        var ownerName = $"{owner.FirstName} {owner.LastName}".Trim();
                        var subject = EmailTemplates.PostCommentedSubject(likerName); // likerName is commenter here
                        var body = EmailTemplates.PostCommentedBody(
                            ownerName,
                            likerName,
                            post?.Title ?? "your post",
                            dto?.Body,
                            $"{BaseUrl}/Posts/Details/{postId}"
                        );

                        await mailService.SendEmailAsync(new MailRequestDto
                        {
                            ToEmail = owner.EmailAddress,
                            Subject = subject,
                            Body = body
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending post commented email to onwer: {ex.Message}");
                }
            }

            return Ok(new
            {
                id = comment.Id,
                body = comment.Body,
                createdOn = comment.CreatedOn, // should be UTC Now
                author = author == null ? null : new
                {
                    firstName = author.FirstName,
                    lastName = author.LastName,
                    profilePicturePath = author.ProfilePicturePath
                }
            });
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
