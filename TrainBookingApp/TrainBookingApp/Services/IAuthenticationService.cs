using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface IAuthenticationService
{
    User? Login(string email, string password);
    User? Register(User user, string password);
    bool IsEmailExists(string email);
    void UpdateLastLogin(int userId);
}