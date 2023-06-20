using Backend.BusinessLogic.AuthManagers.Contracts;
using Backend.BusinessLogic.AuthManagers;
using Backend.BusinessLogic.Repositories.AuthRepository;
using Backend.BusinessLogic.Repositories.PostsRepository;
using Backend.BusinessLogic.Repositories.UsersRepository;
using Backend.BusinessLogic.UserContext;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.BusinessLogic
{
    public static class DependencyResolver
    {
        public static IServiceCollection AddApiDependencies(this IServiceCollection services)
        {
            services.AddScoped<IPostsRepository, PostsRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IJwtManager, JwtManager>();
            services.AddScoped<IHashManager, HashManager>();
            services.AddScoped<IPasswordManager, PasswordManager>();

            return services;
        }
    }
}