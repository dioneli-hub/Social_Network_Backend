using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Azure;
using Backend.ApiModels;
using Backend.BusinessLogic.AuthManagers.Contracts;
using Backend.BusinessLogic.UserContext;
using Backend.DataAccess;
using Backend.Domain;
using Microsoft.EntityFrameworkCore;

namespace Backend.BusinessLogic.Repositories.AuthRepository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DatabaseContext _database;
        private readonly IJwtManager _jwtManager;
        private readonly IPasswordManager _passwordManager;
        private readonly IMapper _mapper;

        public AuthRepository(DatabaseContext database, IJwtManager jwtManager, IPasswordManager passwordManager, IMapper mapper)
        {
            _database = database;
            _jwtManager = jwtManager;
            _passwordManager = passwordManager;
            _mapper = mapper;
        }
        public async Task<ServiceResponse<UserModel>> GetAuthenticatedUser(int currentUserId)
        {
            
            var response = new ServiceResponse<UserModel>();

            var authenticatedUser = await _database.Users
                .Include(x => x.Avatar)
                .Include(x => x.UserFollowers)
                .Include(x => x.UserFollowsTo)
                .FirstOrDefaultAsync(x => x.Id == currentUserId);

            var result = _mapper.Map<UserModel>(authenticatedUser);
            response.Data = result;

            return response;
        }

        public async Task<ServiceResponse<string>> Authenticate(string email, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _database.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User is not found.";
            }
            else if (!await _passwordManager.Verify(user.Id, password))
            {
                response.IsSuccess = false;
                response.Message = "Wrong password.";
            }
            else
            {
                response.Data = _jwtManager.GenerateJwtToken(user.Id);
            }

            return response;
        }

    }
}
