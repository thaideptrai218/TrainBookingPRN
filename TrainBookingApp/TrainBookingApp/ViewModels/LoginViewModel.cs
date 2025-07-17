using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public LoginViewModel(IAuthenticationService authService)
    {
        _authService = authService;
        LoginCommand = new RelayCommand(_ => Login(), _ => CanLogin());
        ShowRegisterCommand = new RelayCommand(_ => ShowRegister());
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

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
                CommandManager.InvalidateRequerySuggested();
        }
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

    public ICommand LoginCommand { get; }
    public ICommand ShowRegisterCommand { get; }

    public event Action<User>? LoginSuccessful;
    public event Action? ShowRegisterRequested;

    private bool CanLogin()
    {
        return !IsLoading && 
               !string.IsNullOrWhiteSpace(Email) && 
               !string.IsNullOrWhiteSpace(Password);
    }

    private void Login()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var user = _authService.Login(Email, Password);

            if (user != null)
            {
                LoginSuccessful?.Invoke(user);
            }
            else
            {
                ErrorMessage = "Invalid email or password. Please try again.";
            }
        }
        catch (Exception)
        {
            ErrorMessage = "An error occurred during login. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowRegister()
    {
        ShowRegisterRequested?.Invoke();
    }
}