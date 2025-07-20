using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TrainBookingApp.ViewModels.Manager;
using TrainBookingApp.Models;

namespace TrainBookingApp.Views;

public partial class ManagerWindow : Window
{
    private readonly MainManagerViewModel _viewModel;

    public ManagerWindow(MainManagerViewModel viewModel)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        // Subscribe to events
        _viewModel.LogoutRequested += OnLogoutRequested;
    }

    public void Initialize(User user)
    {
        _viewModel.Initialize(user);
    }

    // Selection change handlers are now handled in the UserControl views

    private void OnLogoutRequested()
    {
        // Get the application's service provider
        var app = (App)Application.Current;
        var loginWindow = app.ServiceProvider.GetRequiredService<LoginWindow>();
        
        loginWindow.Show();
        this.Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        _viewModel.LogoutRequested -= OnLogoutRequested;
        
        base.OnClosed(e);
    }
}