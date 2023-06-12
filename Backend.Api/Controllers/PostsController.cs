using AutoMapper;
using Backend.Api.Infrastructure;
using Backend.ApiModels;
using Backend.BusinessLogic.Repositories.PostsRepository;
using Backend.BusinessLogic.UserContext;
using Backend.DataAccess;
using Backend.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class PostsController : ControllerBase
    {
        private readonly IPostsRepository _postsRepository;
        private readonly IUserContextService _userContextService;

        public PostsController(IPostsRepository postsRepository, IUserContextService userContextService)
        {
            _postsRepository = postsRepository;
            _userContextService = userContextService;
        }

        [HttpGet("news", Name = nameof(GetNews))]
        public async Task<ActionResult<List<PostModel>>> GetNews()
        {
            var currentUserId = _userContextService.GetCurrentUserId();

            var news = await _postsRepository.GetNews(currentUserId);
            return Ok(news);
        }

        [HttpPost(Name = nameof(CreatePost))]
        public async Task<ActionResult<PostModel>> CreatePost(CreatePostModel model)
        {
            var currentUserId = _userContextService.GetCurrentUserId();
            var postId =await _postsRepository.CreatePost(model, currentUserId);
           return Ok(postId);
        }

        [HttpGet("{postId}", Name = nameof(GetPostById))] 
        public async Task<ActionResult<PostModel>> GetPostById(int postId)
        {
            var post = await _postsRepository.GetPostById(postId);
            return post != null ? Ok(post) : NotFound();
        }

        [HttpDelete("{postId}", Name = nameof(RemovePost))]
        public async Task<ActionResult> RemovePost(int postId)
        {
            var result = await _postsRepository.RemovePost(postId);
            return Ok(result);
        }

        [HttpGet("{postId}/comments", Name = nameof(GetPostComments))]
        public async Task<ActionResult<List<PostCommentModel>>> GetPostComments(int postId)
        {
            var comments = await _postsRepository.GetPostComments(postId);
            return Ok(comments); 
        }

        [HttpPost("{postId}/comments", Name = nameof(AddCommentToPost))]
        public async Task<ActionResult<PostCommentModel>> AddCommentToPost(
            [FromBody] AddCommentToPostModel model,
            [FromRoute] int postId)
        {
            var currentUserId = _userContextService.GetCurrentUserId();
            var comment = await _postsRepository.AddCommentToPost(model, postId, currentUserId);
            return Ok(comment); 
        }

        [HttpDelete("{postId}/comments/{commentId}", Name = nameof(RemoveCommentFromPost))]
        public async Task<ActionResult> RemoveCommentFromPost(int postId, int commentId)
        {
            var currentUserId = _userContextService.GetCurrentUserId();
            var result = await _postsRepository.RemoveCommentFromPost(commentId, postId, currentUserId);
            return Ok(result);
        }

        [HttpGet("{postId}/likes", Name = nameof(GetPostLikes))]
        public async Task<ActionResult<List<PostLikeModel>>> GetPostLikes(int postId)
        {
            var likes = await _postsRepository.GetPostLikes(postId);
            return Ok(likes); 
        }

        [HttpPost("{postId}/likes", Name = nameof(AddLikeToPost))]
        public async Task<ActionResult<PostLikeModel>> AddLikeToPost(int postId)
        {
            var currentUserId = _userContextService.GetCurrentUserId();
            var like = await _postsRepository.AddLikeToPost(postId, currentUserId);
            return Ok(like); 
        }

        [HttpDelete("{postId}/likes", Name = nameof(RemoveLikeFromPost))]
        public async Task<ActionResult> RemoveLikeFromPost(int postId)
        {
            var currentUserId = _userContextService.GetCurrentUserId();
            var like = await _postsRepository.RemoveLikeFromPost(postId, currentUserId);
            return Ok(like);
        }
    }
}
