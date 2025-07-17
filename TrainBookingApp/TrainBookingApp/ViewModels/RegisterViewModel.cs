using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _idCardNumber = string.Empty;
    private DateOnly? _dateOfBirth;
    private string _gender = string.Empty;
    private string _address = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public RegisterViewModel(IAuthenticationService authService)
    {
        _authService = authService;
        RegisterCommand = new RelayCommand(_ => Register(), _ => CanRegister());
        ShowLoginCommand = new RelayCommand(_ => ShowLogin());
    }

    public string FullName
    {
        get => _fullName;
        set
        {
            if (SetProperty(ref _fullName, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (SetProperty(ref _phoneNumber, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (SetProperty(ref _confirmPassword, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public string IdCardNumber
    {
        get => _idCardNumber;
        set => SetProperty(ref _idCardNumber, value);
    }

    public DateOnly? DateOfBirth
    {
        get => _dateOfBirth;
        set => SetProperty(ref _dateOfBirth, value);
    }

    public string Gender
    {
        get => _gender;
        set => SetProperty(ref _gender, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public ICommand RegisterCommand { get; }
    public ICommand ShowLoginCommand { get; }

    public event Action<User>? RegistrationSuccessful;
    public event Action? ShowLoginRequested;

    private bool CanRegister()
    {
        return !IsLoading && 
               !string.IsNullOrWhiteSpace(FullName) &&
               !string.IsNullOrWhiteSpace(Email) && 
               !string.IsNullOrWhiteSpace(PhoneNumber) &&
               !string.IsNullOrWhiteSpace(Password) &&
               !string.IsNullOrWhiteSpace(ConfirmPassword) &&
               Password == ConfirmPassword;
    }

    private void Register()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Validate passwords match
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            // Validate email format (basic validation)
            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address.";
                return;
            }

            // Create new user object
            var newUser = new User
            {
                FullName = FullName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                IdcardNumber = string.IsNullOrWhiteSpace(IdCardNumber) ? null : IdCardNumber,
                DateOfBirth = DateOfBirth,
                Gender = string.IsNullOrWhiteSpace(Gender) ? null : Gender,
                Address = string.IsNullOrWhiteSpace(Address) ? null : Address,
                Role = "Customer", // Default role
                IsActive = true,
                CreatedAt = DateTime.Now,
                IsGuestAccount = false
            };

            var registeredUser = _authService.Register(newUser, Password);

            if (registeredUser != null)
            {
                RegistrationSuccessful?.Invoke(registeredUser);
            }
            else
            {
                ErrorMessage = "Registration failed. Email may already be in use.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred during registration. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowLogin()
    {
        ShowLoginRequested?.Invoke();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}