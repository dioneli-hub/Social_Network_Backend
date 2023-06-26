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
        public async Task<List<PostModel>> GetNews(int currentUserId)
        {
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

            return news;
        }

        public async Task<PostModel> CreatePost(CreatePostModel model, int currentUserId)
        {
            var post = new Post
            {
                Text = model.Text,
                AuthorId = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _database.Posts.Add(post);
            await _database.SaveChangesAsync();

            var postModel = await GetPostById(post.Id);
            return postModel;
        }

        public async Task RemovePost(int postId)
        {
            var post = await _database.Posts
                .Include(x => x.Comments)
                .Include(x => x.Likes)
                .FirstOrDefaultAsync(x => x.Id == postId);

                _database.PostComments.RemoveRange(post.Comments);
                _database.PostLikes.RemoveRange(post.Likes);
                _database.Posts.Remove(post);
                await _database.SaveChangesAsync();
        }

        public async Task<PostModel> GetPostById(int postId)
        {
            var post = await _database.Posts
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .FirstOrDefaultAsync(x => x.Id == postId);
            var postModel = _mapper.Map<PostModel>(post);

            return postModel;
        }

        public async Task<List<PostCommentModel>> GetPostComments(int postId)
        {
            var comments = await _database.PostComments
                .Include(x => x.Author)
                .Where(x => x.PostId == postId)
                .OrderBy(x => x.Id)
                .ToListAsync();
            var commentsModel = _mapper.Map<List<PostCommentModel>>(comments);

            return commentsModel;
        }

        public async Task<PostCommentModel> GetPostCommentById(int postId, int commentId)
        {
            var comment = await _database.PostComments
                .Include(x => x.Author)
                .Where(x => x.PostId == postId)
                .FirstOrDefaultAsync(x => x.Id == commentId);

            var commentModel = _mapper.Map<PostCommentModel>(comment);

            return commentModel;
        }

        public async Task<PostCommentModel> AddCommentToPost(AddCommentToPostModel addCommentToPostModel, int postId, int currentUserId)
        {
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

            var returnedCommentModel = _mapper.Map<PostCommentModel>(returnedComment);

            return returnedCommentModel;
        }

        public async Task<bool> PostExists(int postId)
        {
            var hasPost = await _database.Posts.AnyAsync(x => x.Id == postId);
            return hasPost;
        }

        public async Task RemoveCommentFromPost(int commentId, int postId, int currentUserId)
        {
            var comment = await _database.PostComments.FirstOrDefaultAsync(x => x.Id == commentId);

                _database.PostComments.Remove(comment);
                await _database.SaveChangesAsync();
        }

        public async Task<List<PostLikeModel>> GetPostLikes(int postId)
        {
            var likes = await _database.PostLikes
                .Include(x => x.User)
                .Where(x => x.PostId == postId)
                .ToListAsync();

            var likeModels = _mapper.Map<List<PostLikeModel>>(likes);

            return likeModels;
        }

        public async Task<PostLikeModel> GetPostLikeById(int postId, int currentUserId)
        {
            var like = await _database.PostLikes
                .Include(x => x.User)
                .Where(x => x.PostId == postId && x.User.Id == currentUserId )
                .FirstOrDefaultAsync();

            var likeModel = _mapper.Map<PostLikeModel>(like);

            return likeModel;
        }

        //public async Task<PostLikeModel> AddLikeToPost(int postId, int currentUserId)
        //{
        //    var like = await _database.PostLikes.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId);

        //        like = new PostLike
        //        {
        //            UserId = currentUserId,
        //            PostId = postId,
        //            LikedAt = DateTimeOffset.UtcNow
        //        };
        //        await _database.PostLikes.AddAsync(like);
        //        await _database.SaveChangesAsync();

        //    var returnedLike = _mapper.Map<PostLikeModel>(await _database.PostLikes
        //        .Include(x => x.User)
        //        .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId));

        //    return returnedLike;
        //}

        public async Task<PostLikeModel> AddLikeToPost(int postId, int currentUserId)
        {
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

            //var returnedLike = _mapper.Map<PostLikeModel>
            //    (
            //        await _database.PostLikes
            //        .Include(x => x.User)
            //        .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId)
            //    );

            //return returnedLike;

            like = await _database.PostLikes
                           .Include(x => x.User)
                           .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId);

            var likeModel = _mapper.Map<PostLikeModel>(like);

            return likeModel;

        }

        public async Task<PostLike> RemoveLikeFromPost(int postId, int currentUserId)
        {
            var like = await _database.PostLikes.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId);

            _database.PostLikes.Remove(like);
            await _database.SaveChangesAsync();
            return like;
        }
    }
}
