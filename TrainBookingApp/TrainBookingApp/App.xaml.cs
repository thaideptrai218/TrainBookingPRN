using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TrainBookingApp.Models;
using TrainBookingApp.Services;
using TrainBookingApp.ViewModels;
using TrainBookingApp.Views;

namespace TrainBookingApp;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    
    public ServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("ServiceProvider not initialized");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Create and show login window
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // Register DbContext
        services.AddDbContext<Context>();

        // Register services
        services.AddTransient<IPasswordService, PasswordService>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IStationService, StationService>();
        services.AddTransient<IRouteService, RouteService>();
        services.AddTransient<ITripService, TripService>();
        services.AddTransient<ITrainService, TrainService>();

        // Register ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ManagerViewModel>();

        // Register Views
        services.AddTransient<LoginWindow>();
        services.AddTransient<RegisterWindow>();
        services.AddTransient<ManagerWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
