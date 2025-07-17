using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly Context _context;
    private readonly IPasswordService _passwordService;

    public AuthenticationService(Context context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public User? Login(string email, string password)
    {
        try
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == email && u.IsActive);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return null;

            if (!_passwordService.VerifyPassword(password, user.PasswordHash))
                return null;

            UpdateLastLogin(user.UserId);
            return user;
        }
        catch
        {
            return null;
        }
    }

    public User? Register(User user, string password)
    {
        try
        {
            if (IsEmailExists(user.Email))
                return null;

            var hashedPassword = _passwordService.HashPassword(password);
            user.PasswordHash = hashedPassword;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }
        catch
        {
            return null;
        }
    }

    public bool IsEmailExists(string email)
    {
        return _context.Users.Any(u => u.Email == email);
    }

    public void UpdateLastLogin(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user != null)
        {
            user.LastLogin = DateTime.UtcNow;
            _context.SaveChanges();
        }
    }
}