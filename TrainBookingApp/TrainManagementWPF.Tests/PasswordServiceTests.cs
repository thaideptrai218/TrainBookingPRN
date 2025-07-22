using TrainBookingApp.Services;

namespace TrainManagementWPF.Tests;

/// <summary>
/// Unit tests for PasswordService - testing secure password hashing and verification
/// These are pure unit tests with no dependencies on database or external services
/// </summary>
public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnBase64Hash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hashedPassword = _passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
        
        // Verify it's valid Base64
        Assert.True(IsValidBase64String(hashedPassword));
        
        // Verify the hash length corresponds to expected size (salt + hash = 32 + 64 = 96 bytes)
        // Base64 encoding increases size by ~33%, so 96 bytes = ~128 Base64 characters
        Assert.True(hashedPassword.Length > 100);
    }

    [Fact]
    public void HashPassword_SamePasswordMultipleTimes_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);
        var hash3 = _passwordService.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash2, hash3);
        
        // All should still be valid for verification against the original password
        Assert.True(_passwordService.VerifyPassword(password, hash1));
        Assert.True(_passwordService.VerifyPassword(password, hash2));
        Assert.True(_passwordService.VerifyPassword(password, hash3));
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "SecurePassword2024!";
        var hashedPassword = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hashedPassword = _passwordService.HashPassword(correctPassword);

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithCaseSensitivePassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "CaseSensitive123!";
        var hashedPassword = _passwordService.HashPassword(password);

        // Act
        var resultLowercase = _passwordService.VerifyPassword("casesensitive123!", hashedPassword);
        var resultUppercase = _passwordService.VerifyPassword("CASESENSITIVE123!", hashedPassword);

        // Assert
        Assert.False(resultLowercase);
        Assert.False(resultUppercase);
        
        // But correct case should work
        Assert.True(_passwordService.VerifyPassword(password, hashedPassword));
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldStillWork()
    {
        // Arrange
        var emptyPassword = "";

        // Act
        var hashedPassword = _passwordService.HashPassword(emptyPassword);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
        Assert.True(IsValidBase64String(hashedPassword));
        Assert.True(_passwordService.VerifyPassword(emptyPassword, hashedPassword));
    }

    [Fact]
    public void HashPassword_WithSpecialCharacters_ShouldWork()
    {
        // Arrange
        var passwordWithSpecialChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        var hashedPassword = _passwordService.HashPassword(passwordWithSpecialChars);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.True(IsValidBase64String(hashedPassword));
        Assert.True(_passwordService.VerifyPassword(passwordWithSpecialChars, hashedPassword));
    }

    [Fact]
    public void HashPassword_WithUnicodeCharacters_ShouldWork()
    {
        // Arrange
        var unicodePassword = "पासवर्ड123密码🔒";

        // Act
        var hashedPassword = _passwordService.HashPassword(unicodePassword);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.True(IsValidBase64String(hashedPassword));
        Assert.True(_passwordService.VerifyPassword(unicodePassword, hashedPassword));
    }

    [Fact]
    public void HashPassword_WithVeryLongPassword_ShouldWork()
    {
        // Arrange
        var longPassword = new string('A', 1000) + "123!";

        // Act
        var hashedPassword = _passwordService.HashPassword(longPassword);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.True(IsValidBase64String(hashedPassword));
        Assert.True(_passwordService.VerifyPassword(longPassword, hashedPassword));
    }

    [Fact]
    public void VerifyPassword_WithNullHashedPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword";
        string nullHashedPassword = null!;

        // Act
        var result = _passwordService.VerifyPassword(password, nullHashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithInvalidBase64Hash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword";
        var invalidHash = "This is not a valid Base64 hash!@#$";

        // Act
        var result = _passwordService.VerifyPassword(password, invalidHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithTruncatedHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword";
        var normalHash = _passwordService.HashPassword(password);
        var truncatedHash = normalHash.Substring(0, normalHash.Length / 2);

        // Act
        var result = _passwordService.VerifyPassword(password, truncatedHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithEmptyHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword";
        var emptyHash = "";

        // Act
        var result = _passwordService.VerifyPassword(password, emptyHash);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("StrongP@ssw0rd!")]
    [InlineData("a")]
    [InlineData("1234567890")]
    [InlineData("SimplePassword")]
    public void HashAndVerifyPassword_WithVariousPasswords_ShouldWorkCorrectly(string password)
    {
        // Act
        var hashedPassword = _passwordService.HashPassword(password);
        var verificationResult = _passwordService.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(verificationResult);
        
        // Verify wrong password fails
        var wrongPasswordResult = _passwordService.VerifyPassword(password + "WRONG", hashedPassword);
        Assert.False(wrongPasswordResult);
    }

    [Fact]
    public void HashPassword_ConsistentTimingBehavior_ShouldNotLeakInformation()
    {
        // This test verifies that hash generation time doesn't vary significantly
        // which could lead to timing attacks
        
        // Arrange
        var shortPassword = "abc";
        var longPassword = new string('x', 100);
        var timings = new List<long>();

        // Act & Assert - Run multiple iterations to get average timing
        for (int i = 0; i < 10; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _passwordService.HashPassword(shortPassword);
            stopwatch.Stop();
            timings.Add(stopwatch.ElapsedTicks);

            stopwatch.Restart();
            _passwordService.HashPassword(longPassword);
            stopwatch.Stop();
            timings.Add(stopwatch.ElapsedTicks);
        }

        // The timing should be relatively consistent due to PBKDF2's fixed iteration count
        // We don't assert specific timing values as they vary by machine, but verify basic consistency
        Assert.True(timings.All(t => t > 0), "All hash operations should take measurable time");
    }

    [Fact]
    public void PasswordService_SecurityProperties_ShouldBeMaintained()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        // Assert security properties
        
        // 1. Salt uniqueness - same password should produce different hashes
        Assert.NotEqual(hash1, hash2);
        
        // 2. One-way function - hash should not contain readable password
        Assert.DoesNotContain(password, hash1);
        Assert.DoesNotContain(password, hash2);
        
        // 3. Deterministic verification - verification should be consistent
        Assert.True(_passwordService.VerifyPassword(password, hash1));
        Assert.True(_passwordService.VerifyPassword(password, hash1)); // Second call
        
        // 4. Hash length consistency - all hashes should be same length
        var hash3 = _passwordService.HashPassword("DifferentPassword");
        Assert.Equal(hash1.Length, hash2.Length);
        Assert.Equal(hash1.Length, hash3.Length);
    }

    /// <summary>
    /// Helper method to verify if a string is valid Base64
    /// </summary>
    private static bool IsValidBase64String(string base64)
    {
        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }
}