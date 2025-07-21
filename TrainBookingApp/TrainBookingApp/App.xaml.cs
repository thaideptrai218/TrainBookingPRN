using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TrainBookingApp.Models;
using TrainBookingApp.Services;
using TrainBookingApp.ViewModels;
using TrainBookingApp.ViewModels.Manager;
using TrainBookingApp.Views;
using TrainBookingApp.Views.Manager;

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
        // Register DbContext as Scoped to avoid threading issues
        services.AddDbContext<Context>(ServiceLifetime.Scoped);

        // Register services
        services.AddTransient<IPasswordService, PasswordService>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IStationService, StationService>();
        services.AddTransient<IRouteService, RouteService>();
        services.AddTransient<ITripService, TripService>();
        services.AddTransient<ITrainService, TrainService>();
        services.AddTransient<ITrainTypeService, TrainTypeService>();
        services.AddTransient<ICoachService, CoachService>();
        services.AddTransient<ISeatService, SeatService>();
        services.AddTransient<ICoachTypeService, CoachTypeService>();
        services.AddTransient<ISeatTypeService, SeatTypeService>();
        services.AddTransient<IPricingRuleService, PricingRuleService>();
        services.AddTransient<IPassengerTypeService, PassengerTypeService>();
        services.AddTransient<IBookingService, BookingService>();

        // Register ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<CustomerViewModel>();
        services.AddTransient<SeatSelectionViewModel>();
        services.AddTransient<PassengerDetailsViewModel>();
        services.AddTransient<BookingConfirmationViewModel>();
        
        // Register Manager ViewModels
        services.AddTransient<StationManagementViewModel>();
        services.AddTransient<TrainTypeManagementViewModel>();
        services.AddTransient<CoachTypeManagementViewModel>();
        services.AddTransient<SeatTypeManagementViewModel>();
        services.AddTransient<TrainManagementViewModel>();
        services.AddTransient<RouteManagementViewModel>();
        services.AddTransient<TripManagementViewModel>();
        services.AddTransient<PricingRuleManagementViewModel>();
        services.AddTransient<MainManagerViewModel>();

        // Register Views
        services.AddTransient<LoginWindow>();
        services.AddTransient<RegisterWindow>();
        services.AddTransient<CustomerWindow>();
        services.AddTransient<SeatSelectionWindow>();
        services.AddTransient<PassengerDetailsWindow>();
        services.AddTransient<BookingConfirmationWindow>();
        services.AddTransient<ManagerWindow>();
        
        // Register Manager Views
        services.AddTransient<StationManagementView>();
        services.AddTransient<TrainTypeManagementView>();
        services.AddTransient<CoachTypeManagementView>();
        services.AddTransient<SeatTypeManagementView>();
        services.AddTransient<TrainManagementView>();
        services.AddTransient<RouteManagementView>();
        services.AddTransient<TripManagementView>();
        services.AddTransient<PricingRuleManagementView>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
