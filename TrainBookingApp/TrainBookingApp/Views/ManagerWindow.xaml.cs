using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TrainBookingApp.ViewModels;

namespace TrainBookingApp.Views;

public partial class ManagerWindow : Window
{
    private readonly ManagerViewModel _viewModel;

    public ManagerWindow(ManagerViewModel viewModel)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        // Subscribe to events
        _viewModel.LogoutRequested += OnLogoutRequested;
    }

    private void StationDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel.SelectedStation != null)
        {
            // Populate form with selected station data
            _viewModel.NewStationCode = _viewModel.SelectedStation.StationCode;
            _viewModel.NewStationName = _viewModel.SelectedStation.StationName;
            _viewModel.NewStationCity = _viewModel.SelectedStation.City ?? string.Empty;
            _viewModel.NewStationRegion = _viewModel.SelectedStation.Region ?? string.Empty;
            _viewModel.NewStationPhone = _viewModel.SelectedStation.PhoneNumber ?? string.Empty;
            _viewModel.NewStationAddress = _viewModel.SelectedStation.Address ?? string.Empty;
        }
    }

    private void RouteDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel.SelectedRoute != null)
        {
            // Populate form with selected route data
            _viewModel.NewRouteName = _viewModel.SelectedRoute.RouteName;
            _viewModel.NewRouteDescription = _viewModel.SelectedRoute.Description ?? string.Empty;
        }
    }

    private void TripDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel.SelectedTrip != null)
        {
            // Populate form with selected trip data
            _viewModel.TripDepartureDate = _viewModel.SelectedTrip.DepartureDateTime.Date;
            _viewModel.TripDepartureTime = _viewModel.SelectedTrip.DepartureDateTime.TimeOfDay;
            _viewModel.TripArrivalTime = _viewModel.SelectedTrip.ArrivalDateTime.TimeOfDay;
            _viewModel.BasePriceMultiplier = _viewModel.SelectedTrip.BasePriceMultiplier;
            _viewModel.IsHolidayTrip = _viewModel.SelectedTrip.IsHolidayTrip;
            
            // Set selected route and train
            _viewModel.SelectedRoute = _viewModel.Routes.FirstOrDefault(r => r.RouteId == _viewModel.SelectedTrip.RouteId);
            _viewModel.SelectedTrain = _viewModel.Trains.FirstOrDefault(t => t.TrainId == _viewModel.SelectedTrip.TrainId);
        }
    }

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