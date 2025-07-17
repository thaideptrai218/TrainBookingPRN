using System.Security.Cryptography;
using System.Text;

namespace TrainBookingApp.Services;

/// <summary>
/// Implementation of password hashing service using SHA-256 with salt
/// In Java, this would be similar to using MessageDigest or BCrypt
/// </summary>
public class PasswordService : IPasswordService
{
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 64; // 512 bits
    private const int Iterations = 10000; // Number of iterations for PBKDF2

    /// <summary>
    /// Hashes a password using PBKDF2 with SHA-256
    /// C# equivalent of Java's PBKDF2WithHmacSHA256
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Base64 encoded hash with salt</returns>
    public string HashPassword(string password)
    {
        // Generate random salt (like Java's SecureRandom)
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash password with salt using PBKDF2
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);
            
            // Combine salt and hash (similar to Java's byte array concatenation)
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
            
            return Convert.ToBase64String(hashBytes);
        }
    }

    /// <summary>
    /// Verifies a password against a hash
    /// C# equivalent of Java's MessageDigest.isEqual() or BCrypt.checkpw()
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hashedPassword">Previously hashed password</param>
    /// <returns>True if password matches</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            // Extract salt and hash from stored hash
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            
            // Extract salt (first 32 bytes)
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            
            // Extract hash (remaining bytes)
            byte[] storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);
            
            // Hash the input password with the same salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(HashSize);
                
                // Compare hashes (similar to Java's Arrays.equals())
                return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
            }
        }
        catch
        {
            return false;
        }
    }
}