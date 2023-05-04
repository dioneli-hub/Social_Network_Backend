using AutoMapper;
using Backend.Api.ApiModels;
using Backend.DataAccess;
using Backend.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class PostsController : ControllerBase
    {
        private readonly DatabaseContext _database;
        private readonly IMapper _mapper;

        public PostsController(DatabaseContext database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        [HttpGet("news", Name = nameof(GetNews))]
        public ActionResult<List<PostModel>> GetNews()
        {
            var posts = _database.Users
                .Where(x => x.Id == CurrentUserId)
                .SelectMany(x => x.UserFollowsTo.SelectMany(f => f.User.Posts))
                .Union(_database.Posts.Where(x => x.AuthorId == CurrentUserId))
                .Distinct()
                
    
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .OrderByDescending(x => x.Id)
                //.Include(x => x.Likes)
                //.Include(x => x.Comments)
                .ToList();

            var projections = _mapper.Map<List<PostModel>>(posts);

            //return Ok(_mapper.Map<PostModel>(posts)); // add an automapper Mapper.Map<List<Person>, List<PersonView>>(people);
            return Ok(projections);
            //return Ok(posts);
        }

        [HttpPost(Name = nameof(CreatePost))]
        public ActionResult<PostModel> CreatePost(CreatePostModel model)
        {
            var post = new Post
            {
                Text = model.Text,
                AuthorId = CurrentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _database.Posts.Add(post);
            _database.SaveChanges();

            return GetPostById(post.Id); 
        }

        [HttpGet("{postId}", Name = nameof(GetPostById))]
        public ActionResult<PostModel> GetPostById(int postId)

        {
            var post = _database.Posts
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .FirstOrDefault(x => x.Id == postId);

            return post != null ? Ok(_mapper.Map<PostModel>(post)) : NotFound(); 
        }

        [HttpDelete("{postId}", Name = nameof(RemovePost))]
        public ActionResult RemovePost(int postId)
        {
            var post = _database.Posts
                .Include(x => x.Comments)
                .Include(x => x.Likes)
                .FirstOrDefault(x => x.Id == postId);
            if (post == null)
            {
                return NotFound();
            }

            _database.PostComments.RemoveRange(post.Comments);
            _database.PostLikes.RemoveRange(post.Likes);
            _database.Posts.Remove(post);
            _database.SaveChanges();

            return Ok();
        }

        [HttpGet("{postId}/comments", Name = nameof(GetPostComments))]
        public ActionResult<List<PostCommentModel>> GetPostComments(int postId)
        {
            var comments = _database.PostComments
                .Include(x => x.Author)
                .Where(x => x.PostId == postId)
                .OrderBy(x => x.Id)
                .ToList();

            return Ok(_mapper.Map<List<PostCommentModel>>(comments)); 
        }

        [HttpPost("{postId}/comments", Name = nameof(AddCommentToPost))]
        public ActionResult<PostCommentModel> AddCommentToPost(
            [FromBody] AddCommentToPostModel model,
            [FromRoute] int postId)
        {

            var hasPost = _database.Posts.Any(x => x.Id == postId);
            if (!hasPost)
            {
                return NotFound();
            }

            var comment = new PostComment
            {
                AuthorId = CurrentUserId,
                PostId = postId,
                Text = model.Text,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _database.PostComments.Add(comment);
            _database.SaveChanges();

            comment = _database.PostComments.Where(x => x.Id == comment.Id)
                .Include(x => x.Author)
                .First();

            return Ok(_mapper.Map<PostCommentModel>(comment)); 
        }

        [HttpDelete("{postId}/comments/{commentId}", Name = nameof(RemoveCommentFromPost))]
        public ActionResult RemoveCommentFromPost(int postId, int commentId)
        {
            var comment = _database.PostComments.FirstOrDefault(x => x.Id == commentId);
            if (comment == null)
            {
                return NotFound();
            }

            if (comment.PostId != postId || comment.AuthorId != CurrentUserId) 
            {
                return Forbid();
            }

            _database.PostComments.Remove(comment);
            _database.SaveChanges();

            return Ok();
        }

        [HttpGet("{postId}/likes", Name = nameof(GetPostLikes))]
        public ActionResult<List<PostLikeModel>> GetPostLikes(int postId)
        {
            var likes = _database.PostLikes
                .Include(x => x.User)
                .Where(x => x.PostId == postId)
                .ToList();
            return Ok(_mapper.Map<List<PostLikeModel>>(likes)); 
        }

        [HttpPost("{postId}/likes", Name = nameof(AddLikeToPost))]
        public ActionResult<PostLikeModel> AddLikeToPost(int postId)
        {
            var hasPost = _database.Posts.Any(x => x.Id == postId);
            if (!hasPost)
            {
                return NotFound();
            }

            var like = _database.PostLikes.FirstOrDefault(x => x.PostId == postId && x.UserId == CurrentUserId);
            if (like == null)
            {
                like = new PostLike
                {
                    UserId = CurrentUserId,
                    PostId = postId,
                    LikedAt = DateTimeOffset.UtcNow
                };
                _database.PostLikes.Add(like);
                _database.SaveChanges();
            }

            like = _database.PostLikes
                .Include(x => x.User) 
                .FirstOrDefault(x => x.PostId == postId && x.UserId == CurrentUserId);

            return Ok(_mapper.Map<PostLikeModel>(like)); 
        }

        [HttpDelete("{postId}/likes", Name = nameof(RemoveLikeFromPost))]
        public ActionResult RemoveLikeFromPost(int postId)
        {
            var like = _database.PostLikes.FirstOrDefault(x => x.PostId == postId && x.UserId == CurrentUserId);

            if (like == null)
            {
                return NotFound();
            }

            _database.PostLikes.Remove(like);
            _database.SaveChanges();

            return Ok();
        }

        public int CurrentUserId
        {
            get
            {
                var nameClaim = HttpContext.User.Identity!.Name;
                return int.Parse( nameClaim! );
            }
        }


    }
}
