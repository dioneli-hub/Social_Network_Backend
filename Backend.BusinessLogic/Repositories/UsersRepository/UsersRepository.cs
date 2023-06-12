using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Backend.ApiModels;
using Backend.BusinessLogic.AuthManagers;
using Backend.BusinessLogic.AuthManagers.Contracts;
using Backend.BusinessLogic.UserContext;
using Backend.DataAccess;
using Backend.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.BusinessLogic.Repositories.UsersRepository
{
    public class UsersRepository : IUsersRepository
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IMapper _mapper;
        private readonly IHashManager _hashManager;

        public UsersRepository(DatabaseContext databaseContext, IMapper mapper, IHashManager hashManager)
        {
            _databaseContext = databaseContext;
            _mapper = mapper;
            _hashManager = hashManager;
        }

        public async Task<ServiceResponse<List<SimpleUserModel>>> GetAllUsers()
        {
            var response = new ServiceResponse<List<SimpleUserModel>>();
            var users = await _databaseContext.Users
                .Include(x => x.Avatar)
                .ToListAsync();
            var usersModel = _mapper.Map<List<SimpleUserModel>>(users);

            response.Data = usersModel;

            return response;
        }

        public async Task<ServiceResponse<UserModel>> GetUserById(int userId)
        {
            var response = new ServiceResponse<UserModel>();
            var user = await _databaseContext.Users
                .Include(x => x.Avatar)
                .Include(x => x.UserFollowers)
                .Include(x => x.UserFollowsTo)
                .FirstOrDefaultAsync(x => x.Id == userId);

            var userModel = _mapper.Map<UserModel>(user);
            response.Data = userModel;

            return response;
        }

        public async Task<ServiceResponse<int>> RegisterUser(CreateUserModel model)
        {
            var response = new ServiceResponse<int>();
            var hasAnyByEmail = await _databaseContext.Users.AnyAsync(x => x.Email == model.Email);
            if (hasAnyByEmail)
            {
                response.IsSuccess = false;
                response.Message = "The user with such an email already exists. Please, try another one.";
            }
            else
            {

                var hashModel = _hashManager.Generate(model.Password);
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = Convert.ToBase64String(hashModel.Hash),
                    SaltHash = Convert.ToBase64String(hashModel.Salt),
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _databaseContext.Users.AddAsync(user);
                await _databaseContext.SaveChangesAsync();

                var createdUserResponse = await GetUserById(user.Id);
                var createdUserId = createdUserResponse.Data.Id;

                response.Data = createdUserId;
                response.IsSuccess = true;
                response.Message = "The user has been successfully registered!";
            }
           
            return response; 
        }

        public async Task<ServiceResponse<List<PostModel>>> GetUserPosts(int userId) 
        {
            var response = new ServiceResponse<List<PostModel>>();
            var posts = await _databaseContext.Posts
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .Where(x => x.AuthorId == userId)
                .ToListAsync();
            var postsModel = _mapper.Map<List<PostModel>>(posts); 

            response.Data = postsModel;
            response.IsSuccess = true;

            return response;
        }

        public async Task<ServiceResponse<List<SimpleUserModel>>> GetUserFollowers(int userId, int? limit) //Does not work, fix needed!
        {
            var response = new ServiceResponse<List<SimpleUserModel>>();
            var usersQuery = _databaseContext.UserFollowers
                .Include(x => x.User)
                .ThenInclude(x => x.Avatar)
                .Where(x => x.UserId == userId)
                .Select(x => x.Follower)
                .AsQueryable();

            if (limit.HasValue)
            {
                usersQuery = usersQuery.Take(limit.Value);
            }
            var users = usersQuery.ToListAsync();

            var usersFollowersModel = _mapper.Map<List<SimpleUserModel>>(users); 

            response.Data = usersFollowersModel;
            response.IsSuccess = true;

            return response; 
        }

        public async Task<ServiceResponse<List<SimpleUserModel>>> GetUserFollowsTo(int userId, int? limit) //Does not work, fix needed!
        {
            var response = new ServiceResponse<List<SimpleUserModel>>();
            var usersQuery = _databaseContext.UserFollowers
                .Include(x => x.User)
                .ThenInclude(x => x.Avatar)
                .Where(x => x.FollowerId == userId)
                .Select(x => x.User)
                .AsQueryable();

            if (limit.HasValue)
            {
                usersQuery = usersQuery.Take(limit.Value);
            }

            var users = usersQuery.ToListAsync();
            var usersModel = _mapper.Map<List<SimpleUserModel>>(users); 

            response.Data = usersModel;
            response.IsSuccess = true;
            response.Message = "Here are the users that this user follows.";

            return response;
        }

        public bool HasFollow(int userId, int followToUserId)
        {
            return  _databaseContext.UserFollowers
                .Any(x => x.FollowerId == userId &&
                          x.UserId == followToUserId);
        }

        public async Task<ServiceResponse<bool>> FollowToUser(int userId, int followToUserId, int currentUserId)
        {
            var response = new ServiceResponse<bool>();
            
            if (currentUserId != userId)
            {
                response.IsSuccess = false;
                response.Message = "Cannot follow as another user.";
            }

            var hasFollow = HasFollow(userId, followToUserId);
            if (!hasFollow)
            {
                var user = _databaseContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
                var followToUser = _databaseContext.Users.FirstOrDefaultAsync(x => x.Id == followToUserId);

                if (user == null || followToUser == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found.";
                }

                var following = new UserFollower
                {
                    FollowerId = userId,
                    UserId = followToUserId
                };
                await _databaseContext.UserFollowers.AddAsync(following);
                await _databaseContext.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "Successfully started following the user.";
            }
            return response;
        }

        public async Task<bool> HasFollowTo(int userId, int followToUserId)
        {
            var hasFollow = HasFollow(userId, followToUserId);
            return hasFollow;
        }

        public async Task<ServiceResponse<bool>> UnfollowFromUser(int userId, int unfollowUserId)
        {
            var response = new ServiceResponse<bool>();
            var following = await _databaseContext.UserFollowers
                .FirstOrDefaultAsync(x => x.UserId == unfollowUserId &&
                                     x.FollowerId == userId);
            if (following != null)
            {
                _databaseContext.UserFollowers.Remove(following);
                await _databaseContext.SaveChangesAsync();

                response.Data = true;
                response.IsSuccess = true;
                response.Message = "Unfollowed this user.";
            } else

            {
                response.Data = false;
                response.IsSuccess = false;
                response.Message = "Cannot unfollow an unfollowed user :((((";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> UploadAvatar(IFormFile file, int currentUserId)
        {
            var response = new ServiceResponse<bool>();
            var user = await _databaseContext.Users
                .Include(x => x.Avatar)
                .FirstOrDefaultAsync(x => x.Id == currentUserId);

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
                _databaseContext.ApplicationFiles.Remove(user.Avatar);
                user.AvatarFileId = null;
            }

            user.Avatar = applicationFile;
            await _databaseContext.ApplicationFiles.AddAsync(applicationFile);
            await _databaseContext.SaveChangesAsync();

            response.Data = true;
            response.IsSuccess = true;
            response.Message = "Successfully uploaded an avatar!";

            return response;
        }
    }
}
