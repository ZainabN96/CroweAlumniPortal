using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Models;

namespace CroweAlumniPortal.Data.IServices
{
    public interface IPostService
    {
        Task<Post> CreateAsync(PostDto dto, int? createdBy);
        Task<List<Post>> GetLatestAsync(int take = 10);
        Task LikeAsync(int postId, int userId);
        Task UnlikeAsync(int postId, int userId);
        Task<int> GetLikeCountAsync(int postId);
        Task<PostComment> AddCommentAsync(CommentDto dto, int userId);
        Task<List<PostComment>> GetCommentsAsync(int postId);
        Task<List<PostWithMetaDto>> QueryLatestWithMetaAsync(int take = 10);
        Task<bool> HasUserLikedAsync(int postId, int userId);
        Task<List<object>> QueryLatestCompactAsync(int take = 10);
        Task<int?> GetOwnerIdAsync(int postId);
        Task<Post?> GetByIdAsync(int id);
    }
}
