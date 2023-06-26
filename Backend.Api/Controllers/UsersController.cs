using Backend.ApiModels;
using Backend.BusinessLogic.Repositories.UsersRepository;
using Backend.BusinessLogic.UserContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IUserContextService _userContextService;

        public UsersController(IUsersRepository usersRepository, IUserContextService userContextService)
        {
            _usersRepository = usersRepository;
            _userContextService = userContextService;
        }

        [HttpGet(Name = nameof(GetAllUsers))]
        [ProducesResponseType(typeof(List<SimpleUserModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SimpleUserModel>>> GetAllUsers()
        {
            var users = await _usersRepository.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public async Task<ActionResult<UserModel>> GetUserById(int userId)
        {
            var user = await _usersRepository.GetUserById(userId);
            return Ok(user);
        }

        [HttpPost(Name = nameof(RegisterUser))] 
        [AllowAnonymous]
        public async Task<ActionResult<UserModel>> RegisterUser(CreateUserModel model)
        {
            var createdUserModel = await _usersRepository.RegisterUser(model);
            return Ok(createdUserModel);
        }

        [HttpGet("{userId}/posts", Name = nameof(GetUserPosts))]
        public async Task<ActionResult<List<PostModel>>> GetUserPosts(int userId)
        {
          var postModels = await _usersRepository.GetUserPosts(userId);
          return Ok(postModels);
        }

        [HttpGet("{userId}/followers", Name = nameof(GetUserFollowers))]
        public async Task<ActionResult<List<SimpleUserModel>>> GetUserFollowers(int userId, int? limit)
        {
            var followerModels = await _usersRepository.GetUserFollowers(userId, limit);
            return Ok(followerModels);
        }

        [HttpGet("{userId}/follow-to", Name = nameof(GetUserFollowsTo))]
        public async Task<ActionResult<List<SimpleUserModel>>> GetUserFollowsTo(int userId, int? limit)
        {
            var userModels = await _usersRepository.GetUserFollowsTo(userId, limit);
            return Ok(userModels);
        }

        [HttpPost("{userId}/follow-to/{followToUserId}", Name = nameof(FollowUser))]
        public async Task<ActionResult> FollowUser(int userId, int followToUserId)
        {
            var currentUserId = _userContextService.GetCurrentUserId();

            if (currentUserId != userId)
            {
                return BadRequest("Cannot follow as another user.");
            }

            var hasFollow = await _usersRepository.DoesFollow(userId, followToUserId);

            if (hasFollow)
            {
                return BadRequest("Current user is already followed.");
            }

            var userExists = await _usersRepository.UserByIdExists(userId);
            var followUserExists = await _usersRepository.UserByIdExists(followToUserId);

            if (!userExists || !followUserExists)
            {
                return BadRequest("User does not exist...");
            }

            var userFollowed = await _usersRepository.FollowUser(userId, followToUserId);

            if (!userFollowed)
            {
                return BadRequest("Something went wrong...");
            }

            return Ok("User successfully followed.");

        }

        [HttpGet("{followerId}/follow-to/{userId}", Name = nameof(IsFollowedBy))]
        public async Task<ActionResult<bool>> IsFollowedBy(int followerId, int userId)
        {
            var isFollowedBy = await _usersRepository.IsFollowedBy(followerId, userId);
            return Ok(isFollowedBy);
        }

        [HttpDelete("{userId}/follow-to/{unfollowUserId}", Name = nameof(UnfollowUser))]
        public async Task<ActionResult<bool>> UnfollowUser(int userId, int unfollowUserId)
        {

            var hasUnfollowed = await _usersRepository.UnfollowUser(userId, unfollowUserId);

            if (!hasUnfollowed)
            {
                return BadRequest("Cannot unfollow a not followed user :((((");
            }

            return Ok("User successfully unfollowed.");
        }

        [HttpPost("avatar", Name = nameof(UploadAvatar))]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var currentUserId = _userContextService.GetCurrentUserId();
            var applicationFile = await _usersRepository.UploadAvatar(file, currentUserId);

            return Ok(applicationFile);
        }
    }
}