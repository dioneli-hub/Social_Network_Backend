using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Configuration.Annotations;
using Backend.Api.ApiModels;
using Backend.Api.Managers;
using Backend.DataAccess;
using Backend.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _database;

        public UsersController(DatabaseContext database)
        {
            _database = database;
        }

        [HttpGet(Name = nameof(GetAllUsers))]
        [ProducesResponseType(typeof(List<SimpleUserModel>), StatusCodes.Status200OK)]
        public ActionResult<List<SimpleUserModel>> GetAllUsers()
        {
            var users = _database.Users
                .Include(x => x.Avatar)
                .ToList();
            return Ok(users.ToList()); //Mapper!
        }

        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public ActionResult<UserModel> GetUserById(int userId)
        {
            var user = _database.Users
               
                .Include(x => x.Avatar)
                //when mapper is included, add the following
               //.Include(x => x.UserFollowers)
               //.Include(x => x.UserFollowsTo)
               .FirstOrDefault(x => x.Id == userId);
            return Ok(user);//Mapper!
        }

        [HttpPost(Name = nameof(RegisterUser))]
        [AllowAnonymous]
        public ActionResult<UserModel> RegisterUser(CreateUserModel model)
        {
            var hasAnyByEmail = _database.Users.Any(x => x.Email == model.Email);
            if (hasAnyByEmail)
            {
                return NotFound();
            }

            var hashModel = HashManager.Generate(model.Password);
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = Convert.ToBase64String(hashModel.Hash),
                SaltHash = Convert.ToBase64String(hashModel.Salt),
                CreatedAt = DateTimeOffset.UtcNow
            };

            _database.Users.Add(user);
            _database.SaveChanges();

            return GetUserById(user.Id);
        }

        [HttpGet("{userId}/posts", Name = nameof(GetUserPosts))]
        public ActionResult<PostModel> GetUserPosts(int userId)
        {
            var posts = _database.Posts
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .ToList();
            return Ok(posts.ToList()); //Mapper!
        }

        [HttpGet("{userId}/followers", Name = nameof(GetUserFollowers))]
        public ActionResult<List<SimpleUserModel>> GetUserFollowers(int userId, int? limit)
        {
            var usersQuery = _database.UserFollowers
                .Include(x => x.User)
                .ThenInclude(x => x.Avatar)
                .Where(x => x.UserId == userId)
                .Select(x => x.Follower)
                .AsQueryable();

            if (limit.HasValue)
            {
                usersQuery = usersQuery.Take(limit.Value);
            }

            var users = usersQuery.ToList();

            return Ok(users.ToList()); //Mapper!
        }

        [HttpGet("{userId}/follow-to", Name = nameof(GetUserFollowsTo))]
        public async Task<ActionResult<List<SimpleUserModel>>> GetUserFollowsTo(int userId, int? limit)
        {
            var usersQuery = _database.UserFollowers
                .Include(x => x.User)
                .ThenInclude(x => x.Avatar)
                .Where(x => x.FollowerId == userId)
                .Select(x => x.User)
                .AsQueryable();

            if (limit.HasValue)
            {
                usersQuery = usersQuery.Take(limit.Value);
            }

            var users = usersQuery.ToList();
            return Ok(users.ToList()); //Mapper!
        }

        [HttpPost("{userId}/follow-to/{followToUserId}", Name = nameof(FollowToUser))]
        public ActionResult FollowToUser(int userId, int followToUserId)
        {
            if (CurrentUserId != userId)
            {
                throw new ApplicationException("Cannot follow as another user.");
            }

            var hasFollow = HasFollow(userId, followToUserId);
            if (!hasFollow)
            {
                var user = _database.Users.FirstOrDefaultAsync(x => x.Id == userId);
                var followToUser = _database.Users.FirstOrDefaultAsync(x => x.Id == followToUserId);

                if (user == null || followToUser == null)
                {
                    return NotFound();
                }

                var following = new UserFollower
                {
                    FollowerId = userId,
                    UserId = followToUserId
                };
                _database.UserFollowers.Add(following);
                _database.SaveChanges();
            }

            return Ok();
        }

        private bool HasFollow(int userId, int followToUserId)
        {
            return _database.UserFollowers
                .Any(x => x.FollowerId == userId &&
                               x.UserId == followToUserId);
        }

        [HttpGet("{userId}/follow-to/{followToUserId}", Name = nameof(HasFollowTo))]
        public async Task<ActionResult<bool>> HasFollowTo(int userId, int followToUserId)
        {
            return Ok(HasFollow(userId, followToUserId));
        }

        [HttpDelete("{userId}/follow-to/{unfollowUserId}", Name = nameof(UnfollowFromUser))]
        public ActionResult UnfollowFromUser(int userId, int unfollowUserId)
        {
            var following = _database.UserFollowers
                .FirstOrDefault(x => x.UserId == unfollowUserId &&
                                          x.FollowerId == userId);
            if (following != null)
            {
                _database.UserFollowers.Remove(following);
                _database.SaveChanges();
            }

            return Ok();
        }

        [HttpPost("avatar", Name = nameof(UploadAvatar))]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var user = await _database.Users
                .Include(x => x.Avatar)
                .FirstOrDefaultAsync(x => x.Id == CurrentUserId);

            await using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);

            var applicationFile = new ApplicationFile
            {
                Content = buffer.GetBuffer(),
                ContentType = file.ContentType,
                CreatedAt = DateTimeOffset.UtcNow,
                FileName = file.FileName
            };

            if (user.Avatar != null)
            {
                _database.ApplicationFiles.Remove(user.Avatar);
                user.AvatarFileId = null;
            }

            user.Avatar = applicationFile;
            _database.ApplicationFiles.Add(applicationFile);
            _database.SaveChanges();

            return Ok(applicationFile);
        }

        public int CurrentUserId
        {
            get
            {
                var nameClaim = HttpContext.User.Identity.Name;
                return int.Parse(nameClaim);
            }
        }
    }
}