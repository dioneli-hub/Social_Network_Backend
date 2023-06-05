using System;
using Backend.Api.Managers;
using Backend.DataAccess.Entities;

namespace Backend.Api.Managers
{
    public static class PasswordManager
    {
        public static bool Verify(User user, string password)
        {
            var salt = Convert.FromBase64String(user.SaltHash);
            var hash = HashManager.HashPassword(password, salt);
            var hashedPasswordAsBase64String = Convert.ToBase64String(hash);

            return user.PasswordHash == hashedPasswordAsBase64String;
        }
    }
}