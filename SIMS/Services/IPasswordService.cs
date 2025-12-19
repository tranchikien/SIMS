namespace SIMS.Services
{
    /// <summary>
    /// Interface for password hashing and verification service
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Hashes a plain text password
        /// </summary>
        string HashPassword(string plainPassword);

        /// <summary>
        /// Verifies a plain text password against a hashed password
        /// </summary>
        bool VerifyPassword(string plainPassword, string hashedPassword);
    }
}

