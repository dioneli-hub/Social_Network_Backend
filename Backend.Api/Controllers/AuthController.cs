using AutoMapper;
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
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly DatabaseContext _database;
        private readonly IMapper _mapper;

        public AuthController(DatabaseContext database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet(Name = nameof(GetAuthenticatedUser))]
        public ActionResult<UserModel> GetAuthenticatedUser()
        {
            var user = _database.Users
                .Include(x => x.Avatar)
                .Include(x => x.UserFollowers)
                .Include(x => x.UserFollowsTo)
                .FirstOrDefault(x => x.Id == CurrentUserId);

            return Ok(_mapper.Map<UserModel>(user)); //  mapper check
        }

        [AllowAnonymous]
        [HttpPost(Name = nameof(Authenticate))]
        public ActionResult<TokenModel> Authenticate(AuthenticateModel model)
        {
            var user = _database.Users.FirstOrDefault(x => x.Email == model.Email);
            if (user == null || !PasswordManager.Verify(user, model.Password))
            {
                return Forbid();
            }

            var jwt = JwtManager.GenerateJwtToken(user.Id);
            return Ok(jwt);
        }

        public int CurrentUserId
        {
            get
            {
                var nameClaim = HttpContext.User.Identity!.Name;
                return int.Parse(nameClaim!);

            }
        }
    }
}