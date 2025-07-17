namespace TrainBookingApp.Services;

/// <summary>
/// Service for password hashing and verification operations
/// Similar to Java's BCrypt or MessageDigest functionality
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a plain text password using secure hashing algorithm
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>Hashed password string</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a plain text password against a hashed password
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hashedPassword">Hashed password to compare against</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string password, string hashedPassword);
}