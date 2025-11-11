using CroweAlumniPortal.Data.IServices;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace CroweAlumniPortal.Data.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext dc;
        
        public PostService(ApplicationDbContext dc )
        {
            this.dc = dc;
        }

        public async Task<Post> CreateAsync(PostDto dto, int? createdBy)
        {
            var entity = new Post
            {
                Title = dto.Title.Trim(),
                Body = dto.Body.Trim(),
                MediaPath = dto.MediaPath,
                MediaType = dto.MediaType,
                IsActive = dto.IsActive ?? true,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow,
                LastUpdatedBy = createdBy,
                LastUpdatedOn = DateTime.UtcNow
            };

            dc.Posts.Add(entity);
            await dc.SaveChangesAsync();
            return entity;
        }

        public async Task<List<Post>> GetLatestAsync(int take = 10)
        {
            return await dc.Posts.AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }
        public async Task LikeAsync(int postId, int userId)
        {
            var exists = await dc.PostLikes
                .AnyAsync(x => x.PostId == postId && x.UserId == userId);
            if (exists) return;

            var like = new PostLike
            {
                PostId = postId,
                UserId = userId,
                IsActive = true,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                LastUpdatedBy = userId,
                LastUpdatedOn = DateTime.UtcNow
            };

            dc.PostLikes.Add(like);
            await dc.SaveChangesAsync();
        }

        public async Task UnlikeAsync(int postId, int userId)
        {
            var like = await dc.PostLikes
                .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId);

            if (like == null) return;

            dc.PostLikes.Remove(like);
            await dc.SaveChangesAsync();
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await dc.PostLikes
                .CountAsync(x => x.PostId == postId);
        }

        public async Task<PostComment> AddCommentAsync(CommentDto dto, int userId)
        {
            var postExists = await dc.Posts.AnyAsync(p => p.Id == dto.PostId && p.IsActive);
            if (!postExists) throw new InvalidOperationException("Post not found");

            var c = new PostComment
            {
                PostId = dto.PostId,
                UserId = userId,
                Body = dto.Body?.Trim() ?? "",
                IsActive = true,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                LastUpdatedBy = userId,
                LastUpdatedOn = DateTime.UtcNow
            };

            dc.PostComments.Add(c);
            await dc.SaveChangesAsync();
            return c;
        }

        public async Task<List<PostComment>> GetCommentsAsync(int postId)
        {
            return await dc.PostComments
                .AsNoTracking()
                .Where(c => c.PostId == postId && c.IsActive)
                .OrderBy(c => c.CreatedOn)
                .ToListAsync();
        }
        
        public async Task<List<PostWithMetaDto>> QueryLatestWithMetaAsync(int take = 10)
        {
            var record = dc.Posts
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedOn)
                .Select(p => new PostWithMetaDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    MediaPath = p.MediaPath,
                    MediaType = p.MediaType,
                    CreatedOn = p.CreatedOn,

                    Author = dc.Users
                        .Where(u => u.Id == p.CreatedBy)
                        .Select(u => new UserMiniDto
                        {
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            ProfilePicturePath = u.ProfilePicturePath
                        })
                        .FirstOrDefault(),

                    LikeCount = dc.PostLikes.Count(l => l.PostId == p.Id),

                    Comments = dc.PostComments
                        .Where(c => c.PostId == p.Id)
                        .OrderBy(c => c.CreatedOn)
                        .Select(c => new CommentMiniDto
                        {
                            Id = c.Id,
                            Body = c.Body,
                            CreatedOn = c.CreatedOn,
                            Author = dc.Users
                                .Where(x => x.Id == c.UserId)
                                .Select(x => new UserMiniDto
                                {
                                    FirstName = x.FirstName,
                                    LastName = x.LastName,
                                    ProfilePicturePath = x.ProfilePicturePath
                                })
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .ToListAsync();
            return await record;
        }
        public async Task<bool> HasUserLikedAsync(int postId, int userId)
        {
            return await dc.PostLikes.AnyAsync(x => x.PostId == postId && x.UserId == userId);
        }
        public async Task<List<object>> QueryLatestCompactAsync(int take = 10)
        {
            return await dc.Posts
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedOn)
                .Take(take)
                .Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    body = p.Body,
                    mediaPath = p.MediaPath,
                    mediaType = p.MediaType,
                    createdOn = p.CreatedOn,

                    author = (from u in dc.Users
                              where p.CreatedBy != null && u.Id == p.CreatedBy
                              select new
                              {
                                  firstName = u.FirstName,
                                  lastName = u.LastName,
                                  profilePicturePath = u.ProfilePicturePath
                              }).FirstOrDefault(),

                    likeCount = dc.PostLikes.Count(l => l.PostId == p.Id)
                })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<int?> GetOwnerIdAsync(int postId)
        {
            return await dc.Posts.Where(p => p.Id == postId)
                    .Select(p => p.CreatedBy)
                    .FirstOrDefaultAsync();
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await dc.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task SoftDeleteAsync(int id, string deletedBy)
        {
            var p = await dc.Posts.FindAsync(id);
            if (p == null) throw new KeyNotFoundException("Post not found");
            if (p.IsDeleted) return;

            p.IsDeleted = true;
            p.DeletedOn = DateTime.UtcNow;
            p.DeletedBy = deletedBy;
            p.IsActive = false;
            await dc.SaveChangesAsync();
        }

        public async Task RestoreAsync(long id)
        {
            var p = await dc.Posts.FindAsync(id);
            if (p == null) throw new KeyNotFoundException("Post not found");
            if (!p.IsDeleted) return;

            p.IsDeleted = false;
            p.DeletedOn = null;
            p.DeletedBy = null;
            await dc.SaveChangesAsync();
        }
    }
}
