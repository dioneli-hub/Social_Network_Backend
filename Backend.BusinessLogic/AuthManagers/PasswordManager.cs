using Backend.BusinessLogic.AuthManagers.Contracts;
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
    }
}