using AutoMapper;
using Backend.ApiModels;
using Backend.BusinessLogic.UserContext;
using Backend.DataAccess;
using Backend.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.BusinessLogic.Repositories.PostsRepository
{
    public class PostsRepository : IPostsRepository
    {
        private readonly DatabaseContext _database;
        private readonly IMapper _mapper;

        public PostsRepository(DatabaseContext database, IMapper mapper)
        {
            _mapper = mapper;
            _database = database;
        }
        public async Task<ServiceResponse<List<PostModel>>> GetNews(int currentUserId)
        {
            var response = new ServiceResponse<List<PostModel>>();
            var news = await _database.Users
                .Where(x => x.Id == currentUserId)
                .SelectMany(x => x.UserFollowsTo.SelectMany(f => f.User.Posts))
                .Union(_database.Posts.Where(x => x.AuthorId == currentUserId))
                .Distinct()
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .OrderByDescending(x => x.Id)
                .Select(post => new PostModel
                {
                    Id = post.Id,
                    Text = post.Text,
                    CreatedAt = post.CreatedAt,
                    TotalLikes = post.Likes.Count,
                    TotalComments = post.Comments.Count,
                    Author = _mapper.Map<SimpleUserModel>(post.Author)
                })
                .ToListAsync();

            response.Data = news;
            return response;
        }

        public async Task<ServiceResponse<int>> CreatePost(CreatePostModel model, int currentUserId)
        {
            var response = new ServiceResponse<int>();
            var post = new Post
            {
                Text = model.Text,
                AuthorId = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _database.Posts.Add(post);
            await _database.SaveChangesAsync();

            var postModelResponse = await GetPostById(post.Id);

            response.Data = postModelResponse.Data.Id;
            return response;
        }

        public async Task<ServiceResponse<bool>> RemovePost(int postId)
        {
            var response = new ServiceResponse<bool>();
            var post = await _database.Posts
                .Include(x => x.Comments)
                .Include(x => x.Likes)
                .FirstOrDefaultAsync(x => x.Id == postId);

            if (post == null)
            {
                response.Data = false;
                response.IsSuccess = false;
                response.Message = "Post not found.";
            }
            else
            {
                _database.PostComments.RemoveRange(post.Comments);
                _database.PostLikes.RemoveRange(post.Likes);
                _database.Posts.Remove(post);
                await _database.SaveChangesAsync();

                response.Data = true;
            }
            return response;
        }

        public async Task<ServiceResponse<PostModel>> GetPostById(int postId)
        {
            var response = new ServiceResponse<PostModel>();
            var post = await _database.Posts
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .FirstOrDefaultAsync(x => x.Id == postId);
            var postModel = _mapper.Map<PostModel>(post);

            response.Data = postModel;
            return response;
        }

        public async Task<ServiceResponse<List<PostCommentModel>>> GetPostComments(int postId)
        {
            var response = new ServiceResponse<List<PostCommentModel>>();
            var comments = await _database.PostComments
                .Include(x => x.Author)
                .Where(x => x.PostId == postId)
                .OrderBy(x => x.Id)
                .ToListAsync();
            var commentsModel = _mapper.Map<List<PostCommentModel>>(comments);

            response.Data = commentsModel;
            response.IsSuccess = true;
            response.Message = "Success.";

            return response;
        }

        public async Task<ServiceResponse<PostCommentModel>> AddCommentToPost(AddCommentToPostModel addCommentToPostModel, int postId, int currentUserId)
        {
            var response = new ServiceResponse<PostCommentModel>();
            var hasPost = await _database.Posts.AnyAsync(x => x.Id == postId);

            if (!hasPost)
            {
                response.IsSuccess = false;
                response.Message = "Post not found.";
            }

            var comment = new PostComment
            {
                AuthorId = currentUserId,
                PostId = postId,
                Text = addCommentToPostModel.Text,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _database.PostComments.AddAsync(comment);
            await _database.SaveChangesAsync();

            var returnedComment = await _database.PostComments.Where(x => x.Id == comment.Id)
                .Include(x => x.Author)
                .FirstAsync();

            response.Data = _mapper.Map<PostCommentModel>(returnedComment);

            return response;
        }

        public async Task<ServiceResponse<PostComment>> RemoveCommentFromPost(int commentId, int postId, int currentUserId)
        {
            var response = new ServiceResponse<PostComment>();
            var comment = await _database.PostComments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null)
            {
                response.IsSuccess = false;
                response.Message = "Comment not found.";
            }

            else if (comment.PostId != postId || comment.AuthorId != currentUserId)
            {
                response.IsSuccess = false;
                response.Message = "Restricted.";
            }
            else
            {
                _database.PostComments.Remove(comment);
                await _database.SaveChangesAsync();
                response.IsSuccess = true;
            }
            return response;
        }

        public async Task<ServiceResponse<List<PostLikeModel>>> GetPostLikes(int postId)
        {
            var response = new ServiceResponse<List<PostLikeModel>>();

            var likes = await _database.PostLikes
                .Include(x => x.User)
                .Where(x => x.PostId == postId)
                .ToListAsync();

            var likeModels = _mapper.Map<List<PostLikeModel>>(likes);
            response.Data = likeModels;

            return response;
        }

        public async Task<ServiceResponse<PostLikeModel>> AddLikeToPost(int postId, int currentUserId)
        {
            var response = new ServiceResponse<PostLikeModel>();
            var hasPost = await _database.Posts.AnyAsync(x => x.Id == postId);
            if (!hasPost)
            {
                response.IsSuccess = false;
                response.Message = "Post not found.";
            }

            var like = await _database.PostLikes.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId);
            if (like == null)
            {
                like = new PostLike
                {
                    UserId = currentUserId,
                    PostId = postId,
                    LikedAt = DateTimeOffset.UtcNow
                };
                await _database.PostLikes.AddAsync(like);
                await _database.SaveChangesAsync();
            }

            var returnedLike = _mapper.Map<PostLikeModel>(await _database.PostLikes
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId));

            response.Data = returnedLike;

            return response;
        }

        public async Task<ServiceResponse<PostLike>> RemoveLikeFromPost(int postId, int currentUserId)
        {
            var response = new ServiceResponse<PostLike>();
            var like = await _database.PostLikes.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId);
            if (like == null)
            {
                response.IsSuccess = false;
                response.Message = "Not found.";
            }
            else
            {
                _database.PostLikes.Remove(like);
                await _database.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "Success!";
                response.Data = like;
            }
            return response;
        }
    }
}
