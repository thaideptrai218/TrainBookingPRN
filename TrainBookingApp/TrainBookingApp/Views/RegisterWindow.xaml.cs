using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TrainBookingApp.ViewModels;

namespace TrainBookingApp.Views;

public partial class RegisterWindow : Window
{
    public RegisterWindow(RegisterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Subscribe to events
        viewModel.RegistrationSuccessful += OnRegistrationSuccessful;
        viewModel.ShowLoginRequested += OnShowLoginRequested;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        var passwordBox = (PasswordBox)sender;
        var viewModel = (RegisterViewModel)DataContext;
        viewModel.Password = passwordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        var passwordBox = (PasswordBox)sender;
        var viewModel = (RegisterViewModel)DataContext;
        viewModel.ConfirmPassword = passwordBox.Password;
    }

    private void OnRegistrationSuccessful(Models.User user)
    {
        MessageBox.Show($"Account created successfully! Welcome, {user.FullName}!", 
                       "Registration Successful", 
                       MessageBoxButton.OK, 
                       MessageBoxImage.Information);
        
        // Close register window and show login window
        OnShowLoginRequested();
    }

    private void DateOfBirthPicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var datePicker = (DatePicker)sender;
        var viewModel = (RegisterViewModel)DataContext;
        
        if (datePicker.SelectedDate.HasValue)
        {
            viewModel.DateOfBirth = DateOnly.FromDateTime(datePicker.SelectedDate.Value);
        }
        else
        {
            viewModel.DateOfBirth = null;
        }
    }

    private void OnShowLoginRequested()
    {
        // Get the application's service provider
        var app = (App)Application.Current;
        var loginWindow = app.ServiceProvider.GetRequiredService<LoginWindow>();
        
        loginWindow.Show();
        this.Close();
    }
}