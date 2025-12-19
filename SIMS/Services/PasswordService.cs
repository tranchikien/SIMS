using BCrypt.Net;

namespace SIMS.Services
{
    /// <summary>
    /// Implementation of password hashing service using BCrypt
    /// </summary>
    public class PasswordService : IPasswordService
    {
        /// <summary>
        /// Hashes a plain text password using BCrypt
        /// </summary>
        public string HashPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(plainPassword));
            }

            // BCrypt automatically generates a salt and includes it in the hash
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
        }

        /// <summary>
        /// Verifies a plain text password against a hashed password
        /// </summary>
        public bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            try
            {
                // BCrypt automatically extracts the salt from the hash and verifies
                return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
            }
            catch
            {
                // If the hash format is invalid (e.g., old plain text password), return false
                return false;
            }
        }
    }
}

