using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Backend.ApiModels;
using Backend.BusinessLogic.AuthManagers;
using Backend.BusinessLogic.Repositories.UsersRepository;
using Backend.BusinessLogic.UserContext;
using Backend.DataAccess;
using Backend.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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
            var user = await _usersRepository.RegisterUser(model);
            return Ok(user);
        }

        [HttpGet("{userId}/posts", Name = nameof(GetUserPosts))]
        public async Task<ActionResult<List<PostModel>>> GetUserPosts(int userId)
        {
          var posts = await _usersRepository.GetUserPosts(userId);
          return Ok(posts);
        }

        [HttpGet("{userId}/followers", Name = nameof(GetUserFollowers))]
        public async Task<ActionResult<List<SimpleUserModel>>> GetUserFollowers(int userId, int? limit)
        {
            var followers = await _usersRepository.GetUserFollowers(userId, limit);
            return Ok(followers);
        }

        [HttpGet("{userId}/follow-to", Name = nameof(GetUserFollowsTo))]
        public async Task<ActionResult<List<SimpleUserModel>>> GetUserFollowsTo(int userId, int? limit)
        {
            var users = await _usersRepository.GetUserFollowsTo(userId, limit);
            return Ok(users);
        }

        [HttpPost("{userId}/follow-to/{followToUserId}", Name = nameof(FollowToUser))]
        public async Task<ActionResult> FollowToUser(int userId, int followToUserId)
        {
            var currentUserId = _userContextService.GetCurrentUserId();
            var followToUser = await _usersRepository.FollowToUser(userId, followToUserId, currentUserId);
            return Ok(followToUser);
        }

        [HttpGet("{userId}/follow-to/{followToUserId}", Name = nameof(HasFollowTo))]
        public async Task<ActionResult<bool>> HasFollowTo(int userId, int followToUserId)
        {
            var hasFollowTo = await _usersRepository.HasFollowTo(userId, followToUserId);
            return Ok(hasFollowTo);
        }

        [HttpDelete("{userId}/follow-to/{unfollowUserId}", Name = nameof(UnfollowFromUser))]
        public async Task<ActionResult<bool>> UnfollowFromUser(int userId, int unfollowUserId)
        {
            var following = await _usersRepository.UnfollowFromUser(userId, unfollowUserId);
            return Ok(following);
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