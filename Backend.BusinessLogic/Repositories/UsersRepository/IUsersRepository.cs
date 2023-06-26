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
        Task<List<SimpleUserModel>> GetAllUsers();
        Task<UserModel> GetUserById(int userId);
        Task<bool> UserByEmailExists(string email);
        Task<bool> UserByIdExists(int userId);
        Task<ServiceResponse<UserModel>> RegisterUser(CreateUserModel model);
        Task<List<PostModel>> GetUserPosts(int userId);
        Task<List<SimpleUserModel>> GetUserFollowers(int userId, int? limit);
        Task<List<SimpleUserModel>> GetUserFollowsTo(int userId, int? limit);
        Task<bool> DoesFollow(int userId, int followedUserId);
        Task<bool> IsFollowedBy(int followerId, int userId);
        Task<bool> FollowUser(int userId, int followToUserId);
        Task<bool> UnfollowUser(int userId, int unfollowUserId);
        Task<bool> UploadAvatar(IFormFile file, int currentUserId);
    }
}
