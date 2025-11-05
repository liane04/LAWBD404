using System;
using System.Security.Cryptography;
using System.Text;

namespace Marketplace.Services
{
    public static class PasswordHasher
    {
        // Format: PBKDF2$<iterations>$<saltBase64>$<hashBase64>
        private const int SaltSize = 16; // 128-bit
        private const int KeySize = 32;  // 256-bit
        private const int DefaultIterations = 100_000;

        public static string HashPassword(string password, int? iterations = null)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            int iters = iterations ?? DefaultIterations;

            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iters,
                HashAlgorithmName.SHA256,
                KeySize);

            return $"PBKDF2${iters}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(storedHash)) return false;

            var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4 || parts[0] != "PBKDF2") return false;

            if (!int.TryParse(parts[1], out int iterations)) return false;
            byte[] salt;
            byte[] expectedKey;

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedKey = Convert.FromBase64String(parts[3]);
            }
            catch
            {
                return false;
            }

            byte[] actualKey = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedKey.Length);

            // constant time comparison
            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
    }
}

