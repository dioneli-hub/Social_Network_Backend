using AutoMapper;
using Azure;
using Backend.ApiModels;
using Backend.BusinessLogic.AuthManagers;
using Backend.BusinessLogic.AuthManagers.Contracts;
using Backend.DataAccess;
using Backend.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.BusinessLogic.Repositories.UsersRepository
{
    public class UsersRepository : IUsersRepository
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IMapper _mapper;
        private readonly IHashManager _hashManager;
        private readonly IValidationManager _validationManager;

        public UsersRepository(DatabaseContext databaseContext, 
                                        IMapper mapper,
                                        IHashManager hashManager,
                                        IValidationManager validationManager)
        {
            _databaseContext = databaseContext;
            _mapper = mapper;
            _hashManager = hashManager;
            _validationManager = validationManager;
        }

        public async Task<List<SimpleUserModel>> GetAllUsers()
        {
            var users = await _databaseContext.Users
                .Include(x => x.Avatar)
                .ToListAsync();
            var usersModel = _mapper.Map<List<SimpleUserModel>>(users);

            return usersModel;
        }

        public async Task<UserModel> GetUserById(int userId)
        {
            var user = await _databaseContext.Users
                .Include(x => x.Avatar)
                .Include(x => x.UserFollowers)
                .Include(x => x.UserFollowsTo)
                .FirstOrDefaultAsync(x => x.Id == userId);

            var userModel = _mapper.Map<UserModel>(user);

            return userModel;
        }

        public async Task<ServiceResponse<UserModel>> RegisterUser(CreateUserModel model)
        {
            
            var hasAnyByEmail = await UserByEmailExists(model.Email);

            if (hasAnyByEmail)
            {
                return new ServiceResponse<UserModel>()
                {
                    IsSuccess = false,
                    Message = "The user with this email already exists.",
                    Data = null
                };
            }

            var isPasswordValid = _validationManager.ValidatePassword(model.Password);

            if (!isPasswordValid)
            {
                return new ServiceResponse<UserModel>()
                {
                    IsSuccess = false,
                    Message = "Your password is not valid. Make sure it is at lest 8 characters long and contains one uppercase letter, one lowercase letter, one number and one special character ('@', '#', '$', '%', '^', '&', '+', '=', '!', '?').",
                    Data = null
                };
            }

            var isUserEmailValid = _validationManager.ValidateEmail(model.Email);

            if (!isUserEmailValid)
            {
                return new ServiceResponse<UserModel>()
                {
                    IsSuccess = false,
                    Message = "Your email is not valid. Please, check it for correctness.",
                    Data = null
                };
            }
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

            return new ServiceResponse<UserModel>()
            {
                IsSuccess = true,
                Message = "User successfully registered.",
                Data = await GetUserById(user.Id)
            };
        }

        public async Task<bool> UserByEmailExists(string email)
        {
            return await _databaseContext.Users.AnyAsync(x => x.Email == email);
        }

        public async Task<List<PostModel>> GetUserPosts(int userId) 
        {
            var posts = await _databaseContext.Posts
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .Where(x => x.AuthorId == userId)
                .ToListAsync();

            var postModels = _mapper.Map<List<PostModel>>(posts); 

            return postModels;
        }

        public async Task<List<SimpleUserModel>> GetUserFollowers(int userId, int? limit) 
        {
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
            var users = await usersQuery.ToListAsync();

            var usersFollowerModels = _mapper.Map<List<SimpleUserModel>>(users); 

            return usersFollowerModels; 
        }

        public async Task<List<SimpleUserModel>> GetUserFollowsTo(int userId, int? limit) 
        {
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

            var users = await usersQuery.ToListAsync();
            var userModels = _mapper.Map<List<SimpleUserModel>>(users); 

            return userModels;
        }

        public async Task<bool> DoesFollow(int userId, int followedUserId)
        {
            return  await _databaseContext.UserFollowers
                .AnyAsync(x => x.FollowerId == userId &&
                          x.UserId == followedUserId);
        }

        public async Task<bool> FollowUser(int userId, int followToUserId)
        {
            var user = _databaseContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var followToUser = _databaseContext.Users.FirstOrDefaultAsync(x => x.Id == followToUserId);

            var following = new UserFollower
            {
                FollowerId = userId,
                UserId = followToUserId
            };
            await _databaseContext.UserFollowers.AddAsync(following);
            await _databaseContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UserByIdExists(int userId)
        {
            var userExists = await _databaseContext.Users.AnyAsync(x => x.Id == userId);
            return userExists;
        }

        public async Task<bool> IsFollowedBy(int followerId, int userId) 
        {
            var hasFollow = await DoesFollow(followerId, userId);
            return hasFollow;
        }

        public async Task<bool> UnfollowUser(int userId, int unfollowUserId)
        {
            var following = await _databaseContext.UserFollowers
                .FirstOrDefaultAsync(x => x.UserId == unfollowUserId &&
                                     x.FollowerId == userId);
            if (following != null)
            {
                _databaseContext.UserFollowers.Remove(following);
                await _databaseContext.SaveChangesAsync();

                return true;

            }
            return false;
        }

        public async Task<bool> UploadAvatar(IFormFile file, int currentUserId)
        {
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

            return true;
        }
    }
}
