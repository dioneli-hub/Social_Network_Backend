using Backend.ApiModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain;
using Microsoft.AspNetCore.Http;

namespace Backend.BusinessLogic.Repositories.UsersRepository
{
    public interface IUsersRepository
    {
        public Task<ServiceResponse<List<SimpleUserModel>>> GetAllUsers();
        public Task<ServiceResponse<UserModel>> GetUserById(int userId);
        public Task<ServiceResponse<int>> RegisterUser(CreateUserModel model);
        public Task<ServiceResponse<List<PostModel>>> GetUserPosts(int userId);
        public Task<ServiceResponse<List<SimpleUserModel>>> GetUserFollowers(int userId, int? limit);
        public Task<ServiceResponse<List<SimpleUserModel>>> GetUserFollowsTo(int userId, int? limit);
        public bool HasFollow(int userId, int followToUserId);
        public Task<ServiceResponse<bool>> FollowToUser(int userId, int followToUserId, int currentUserId);
        public Task<bool> HasFollowTo(int userId, int followToUserId);
        public Task<ServiceResponse<bool>> UnfollowFromUser(int userId, int unfollowUserId);
        public Task<ServiceResponse<bool>> UploadAvatar(IFormFile file, int currentUserId);
    }
}
