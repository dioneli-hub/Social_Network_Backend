using Backend.Api.ApiModels;
using Backend.DataAccess;
using Backend.DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class PostsController : ControllerBase
    {
        private readonly DatabaseContext _database;

        public PostsController(DatabaseContext database)
        {
            _database = database;
        }

        [HttpPost(Name = nameof(CreatePost))]
        public ActionResult<PostModel> CreatePost(CreatePostModel model)
        {
            var post = new Post
            {
                Text = model.Text,
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
                //later also the author and his/her avatar will be included
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .FirstOrDefault(x => x.Id == postId);

            return post != null ? Ok(post) : NotFound();
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
                // later we will also include the author (as son as create one)
                .Where(x => x.PostId == postId)
                .OrderBy(x => x.Id)
                .ToList();

            return Ok(comments.ToList()); // later we'll modify that with mapper
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
                PostId = postId,
                Text = model.Text,
                CreatedAt = DateTimeOffset.UtcNow
                // do not forget to set the author to the current user
            };
            _database.PostComments.Add(comment);
            _database.SaveChanges();

            comment = _database.PostComments.Where(x => x.Id == comment.Id)
                // later include the author
                .First();

            return Ok(comment); //maybe modify???
        }


        [HttpDelete("{postId}/comments/{commentId}", Name = nameof(RemoveCommentFromPost))]
        public ActionResult RemoveCommentFromPost(int postId, int commentId)
        {
            var comment = _database.PostComments.FirstOrDefault(x => x.Id == commentId);
            if (comment == null)
            {
                return NotFound();
            }

            if (comment.PostId != postId) 
                // later will add validation such that only the author of the comment will be able to delete it
            {
                return Forbid();
            }

            _database.PostComments.Remove(comment);
            _database.SaveChanges();

            return Ok();
        }

        [HttpGet("{postId}/likes", Name = nameof(GetPostLikes))]
        public ActionResult<List<PostCommentModel>> GetPostLikes(int postId)
        {
            var likes = _database.PostLikes
                // include info about the author
                .Where(x => x.Id == postId)
                .ToList();
            return Ok(likes); // change via automapper
        }

        [HttpPost("{postId}/likes", Name = nameof(AddLikeToPost))]
        public ActionResult<PostLikeModel> AddLikeToPost(int postId)
        {
            var hasPost = _database.Posts.Any(x => x.Id == postId);
            if (!hasPost)
            {
                return NotFound();
            }

            var like = _database.PostLikes.FirstOrDefault(x => x.PostId == postId);
            // add validation: check if there is already a like put by the current user


            if (like == null)
            {
                like = new PostLike
                {
                    // set the author to the current user
                    PostId = postId,
                    LikedAt = DateTimeOffset.UtcNow
                };
                _database.PostLikes.Add(like);
                _database.SaveChanges();
            }

            like = _database.PostLikes
                // include info about the author
                .FirstOrDefault(x => x.PostId == postId ); // and the author is the current user

            return Ok(like); //later we'll map with automapper
        }

        [HttpDelete("{postId}/likes", Name = nameof(RemoveLikeFromPost))]
        public ActionResult RemoveLikeFromPost(int postId)
        {
            var like = _database.PostLikes.FirstOrDefault(x => x.PostId == postId ); 
            //check if there exists the like put by the current user (so that noone else could delete it)

            if (like == null)
            {
                return NotFound();
            }

            _database.PostLikes.Remove(like);
            _database.SaveChanges();

            return Ok();
        }


    }
}
