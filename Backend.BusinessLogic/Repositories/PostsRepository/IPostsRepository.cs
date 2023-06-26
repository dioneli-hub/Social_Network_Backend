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
        public Task<List<PostModel>> GetNews(int currentUserId);
        public Task<PostModel> CreatePost(CreatePostModel post, int currentUserId);
        public Task RemovePost(int postId);
        public Task<PostModel> GetPostById(int postId);
        public Task<List<PostCommentModel>> GetPostComments(int postId);
        public Task<bool> PostExists(int postId);
        public Task<PostCommentModel> AddCommentToPost(AddCommentToPostModel postComment, int postId, int currentUserId);
        public Task<PostCommentModel> GetPostCommentById(int postId, int commentId);
        public Task RemoveCommentFromPost(int commentId, int postId, int currentUserId);
        public Task<List<PostLikeModel>> GetPostLikes(int postId);
        public Task<PostLikeModel> GetPostLikeById(int postId, int currentUserId);
        public Task<PostLikeModel> AddLikeToPost(int postId, int currentUserId);
        public Task<PostLike> RemoveLikeFromPost(int postId, int currentUserId);
    }
}
