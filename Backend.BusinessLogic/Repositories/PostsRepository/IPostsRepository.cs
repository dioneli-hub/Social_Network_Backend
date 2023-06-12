using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.ApiModels;
using Backend.Domain;

namespace Backend.BusinessLogic.Repositories.PostsRepository
{
    public interface IPostsRepository
    {
        public Task<ServiceResponse<List<PostModel>>> GetNews(int currentUserId);
        public Task<ServiceResponse<int>> CreatePost(CreatePostModel post, int currentUserId);
        public Task<ServiceResponse<bool>> RemovePost(int postId);
        public Task<ServiceResponse<PostModel>> GetPostById(int postId);
        public Task<ServiceResponse<List<PostCommentModel>>> GetPostComments(int postId);
        public Task<ServiceResponse<PostCommentModel>> AddCommentToPost(AddCommentToPostModel postComment, int postId, int currentUserId);
        public Task<ServiceResponse<PostComment>> RemoveCommentFromPost(int commentId, int postId, int currentUserId);
        public Task<ServiceResponse<List<PostLikeModel>>> GetPostLikes(int postId);
        public Task<ServiceResponse<PostLikeModel>> AddLikeToPost(int postId, int currentUserId);
        public Task<ServiceResponse<PostLike>> RemoveLikeFromPost(int postId, int currentUserId);
    }
}
