﻿using Backend.BusinessLogic.AuthManagers.Contracts;
using Backend.DataAccess;
using Backend.Domain;
using Microsoft.EntityFrameworkCore;

namespace Backend.BusinessLogic.AuthManagers
{
    public class PasswordManager : IPasswordManager
    {
        private readonly DatabaseContext _database;
        private readonly IHashManager _hashManager;

        public PasswordManager(DatabaseContext database, IHashManager hashManager)
        {
            _database = database;
            _hashManager = hashManager;
        }

        public async Task<bool> Verify(int userId, string password)
        {
            var user = await _database.Users.FirstOrDefaultAsync(x => x.Id == userId);
            return Verify(user, password);
        }

        private bool Verify(User user, string password)
        {
            var salt = Convert.FromBase64String(user.SaltHash);
            var hash = _hashManager.HashPassword(password, salt);
            var hashedPasswordAsBase64String = Convert.ToBase64String(hash);

            return user.PasswordHash == hashedPasswordAsBase64String;
        }

        public bool ValidatePassword(string password)
        {
            int validConditions = 0;
            foreach (char c in password)
            {
                if (c >= 'a' && c <= 'z')
                {
                    validConditions++;
                    break;
                }
            }
            foreach (char c in password)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    validConditions++;
                    break;
                }
            }
            if (validConditions < 2) return false;
            foreach (char c in password)
            {
                if (c >= '0' && c <= '9')
                {
                    validConditions++;
                    break;
                }
            }
            if (validConditions < 3) return false;
            if (validConditions == 3)
            {
                char[] special = { '@', '#', '$', '%', '^', '&', '+', '=', '!', '?' };
                if (password.IndexOfAny(special) == -1) return false;

                if (password.Length < 8) return false;
            }

            return true;
        }
    }
}