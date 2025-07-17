using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TrainBookingApp.ViewModels;

namespace TrainBookingApp.Views;


public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        // Set DataContext for data binding (similar to Java's binding setup)
        DataContext = _viewModel;

        // Subscribe to ViewModel events
        _viewModel.LoginSuccessful += OnLoginSuccessful;
        _viewModel.ShowRegisterRequested += OnShowRegisterRequested;

        // Focus on email textbox when window loads
        Loaded += (s, e) => EmailTextBox.Focus();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            // Update ViewModel property when password changes
            _viewModel.Password = passwordBox.Password;
        }
    }

    private void OnLoginSuccessful(Models.User user)
    {
        try
        {
            // Get the application's service provider
            var app = (App)Application.Current;
            
            if (user.Role == "Manager")
            {
                // Show manager dashboard
                var managerWindow = app.ServiceProvider.GetRequiredService<ManagerWindow>();
                managerWindow.Show();
                this.Close();
            }
            else
            {
                // TODO: Show user dashboard for regular users
                MessageBox.Show($"Welcome, {user.FullName}!\nUser dashboard will be implemented next.",
                               "Login Successful",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
                
                // For now, just close the login window
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening dashboard: {ex.Message}",
                           "Error",
                           MessageBoxButton.OK,
                           MessageBoxImage.Error);
        }
    }

    private void OnShowRegisterRequested()
    {
        // Get the application's service provider
        var app = (App)Application.Current;
        var registerWindow = app.ServiceProvider.GetRequiredService<RegisterWindow>();
        
        registerWindow.Show();
        this.Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        _viewModel.LoginSuccessful -= OnLoginSuccessful;
        _viewModel.ShowRegisterRequested -= OnShowRegisterRequested;

        base.OnClosed(e);
    }
}