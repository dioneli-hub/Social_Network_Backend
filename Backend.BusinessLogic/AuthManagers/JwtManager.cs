using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.ApiModels;
using Backend.BusinessLogic.AuthManagers.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace Backend.BusinessLogic.AuthManagers
{
    public class JwtManager : IJwtManager
    {
        public TokenModel GenerateJwtToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var nowUtc = DateTimeOffset.UtcNow;
            var expiresAt = nowUtc.AddDays(30);
            var tokenDescriptor = BuildSecurityTokenDescriptor(userId, nowUtc, expiresAt);
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);

            var result = new TokenModel
            {
                ExpiredAt = expiresAt,
                Token = token
            };
            return result;
        }

        public bool IsValidAuthToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var key = Encoding.ASCII.GetBytes("GDxN28S3JvTRNqzGULCZvH9kzQ8qrxdB");
                var parameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuer = "SocialNetwork.Issuer",
                    ValidateAudience = true,
                    ValidAudience = "SocialNetwork.Audience",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                tokenHandler.ValidateToken(token, parameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private SecurityTokenDescriptor BuildSecurityTokenDescriptor(int userId, DateTimeOffset nowUtc, DateTimeOffset expires)
        {
            var key = Encoding.ASCII.GetBytes("GDxN28S3JvTRNqzGULCZvH9kzQ8qrxdB");
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userId.ToString()),
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "SocialNetwork.Issuer",
                Audience = "SocialNetwork.Audience",
                NotBefore = nowUtc.UtcDateTime,
                Subject = new ClaimsIdentity(claims),
                Expires = expires.UtcDateTime,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenDescriptor;
        }
    }
}