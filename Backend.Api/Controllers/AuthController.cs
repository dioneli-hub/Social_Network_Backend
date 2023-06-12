using AutoMapper;
using Backend.Domain;
using Backend.ApiModels;
using Backend.BusinessLogic.Repositories.AuthRepository;
using Backend.BusinessLogic.UserContext;
using Backend.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserContextService _userContextService;

        public AuthController(IAuthRepository authRepository, IUserContextService userContextService)
        {
            _authRepository = authRepository;
            _userContextService = userContextService;
        }

        [Authorize]
        [HttpGet(Name = nameof(GetAuthenticatedUser))]
        public async Task<ActionResult<ServiceResponse<UserModel>>> GetAuthenticatedUser()
        {
            var currentUserId = _userContextService.GetCurrentUserId();
            var result = await _authRepository.GetAuthenticatedUser(currentUserId);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost(Name = nameof(Authenticate))]
        public async Task<ActionResult<string>> Authenticate(AuthenticateModel model)
        {
            var response = await _authRepository.Authenticate(model.Email, model.Password);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}