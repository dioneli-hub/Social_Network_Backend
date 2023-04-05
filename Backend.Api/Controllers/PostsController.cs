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

        
    }
}
