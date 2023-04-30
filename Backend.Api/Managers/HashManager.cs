using System.Security.Cryptography;
using Backend.Api.Managers;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Backend.Api.Managers
{
    public static class HashManager
    {
        private const int SaltSize = 16;
        private const int IterationsCount = 100;
        private const int KeySizeInBytes = 32;

        public static HashModel Generate(string password)
        {
            var salt = GenerateSalt(SaltSize);

            return new HashModel
            {
                Salt = salt,
                Hash = HashPassword(password, salt)
            };
        }

        public static byte[] HashPassword(string password, byte[] salt)
        {
            return KeyDerivation.Pbkdf2(password, salt,
                KeyDerivationPrf.HMACSHA512,
                IterationsCount,
                KeySizeInBytes);
        }

        private static byte[] GenerateSalt(int saltSize)
        {
            var salt = new byte[saltSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            return salt;
        }
    }
}
