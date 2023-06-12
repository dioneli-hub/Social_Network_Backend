using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.ApiModels;
using Backend.Domain;

namespace Backend.BusinessLogic.Repositories.AuthRepository
{
    public interface IAuthRepository
    {
        Task<ServiceResponse<UserModel>> GetAuthenticatedUser(int currentUserId);
        public Task<ServiceResponse<string>> Authenticate(string email, string password);
    }
}
