using AutoMapper;
using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Data.Services;
using CroweAlumniPortal.Dtos;
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

        public PostsController(IUnitOfWork uow, IFileService files, IMapper mapper, INotificationService notificationService)
        {
            this.uow = uow;
            this.files = files;
            this.mapper = mapper;
            this.notificationService = notificationService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] PostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int? userId = null;
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrWhiteSpace(userIdStr) && int.TryParse(userIdStr, out var parsed))
                userId = parsed;

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

            var author = userId.HasValue ? await uow.UserService.Get(userId.Value) : null;
            var authorName = (author == null)
                ? "Someone"
                : $"{author.FirstName ?? ""} {author.LastName ?? ""}".Trim();

            await notificationService.CreateForAllAsync(new Notification
            {
                Type = "post",
                Title = "New Post Added",
                Message = $"{authorName} added a new post: {dto.Title ?? "Untitled"}",
                Url = "/Dashboard/Index"
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
                        Url = $"/Dashboard/Dashboard#post-{id}"
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
    }
}
