using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class TripManagementViewModel : BaseManagerViewModel
{
    private readonly ITripService _tripService;
    private readonly ITrainService _trainService;
    private readonly IRouteService _routeService;
    
    private ObservableCollection<Trip> _trips = new();
    private Trip? _selectedTrip;
    private ObservableCollection<Train> _trains = new();
    private Train? _selectedTrain;
    private ObservableCollection<Route> _routes = new();
    private Route? _selectedRoute;
    private DateTime _tripDepartureDate = DateTime.Today;
    private TimeSpan _tripDepartureTime = TimeSpan.FromHours(8);
    private TimeSpan _tripArrivalTime = TimeSpan.FromHours(12);
    private decimal _basePriceMultiplier = 1.0m;
    private bool _isHolidayTrip = false;
    private string _searchTerm = string.Empty;

    public TripManagementViewModel(ITripService tripService, ITrainService trainService, IRouteService routeService)
    {
        _tripService = tripService;
        _trainService = trainService;
        _routeService = routeService;
        InitializeCommands();
        RefreshData();
        LoadTrains();
        LoadRoutes();
    }

    public override string TabName => "Trip Management";

    #region Properties

    public ObservableCollection<Trip> Trips
    {
        get => _trips;
        set => SetProperty(ref _trips, value);
    }

    public Trip? SelectedTrip
    {
        get => _selectedTrip;
        set => SetProperty(ref _selectedTrip, value);
    }

    public ObservableCollection<Train> Trains
    {
        get => _trains;
        set => SetProperty(ref _trains, value);
    }

    public Train? SelectedTrain
    {
        get => _selectedTrain;
        set => SetProperty(ref _selectedTrain, value);
    }

    public ObservableCollection<Route> Routes
    {
        get => _routes;
        set => SetProperty(ref _routes, value);
    }

    public Route? SelectedRoute
    {
        get => _selectedRoute;
        set => SetProperty(ref _selectedRoute, value);
    }

    public DateTime TripDepartureDate
    {
        get => _tripDepartureDate;
        set => SetProperty(ref _tripDepartureDate, value);
    }

    public TimeSpan TripDepartureTime
    {
        get => _tripDepartureTime;
        set => SetProperty(ref _tripDepartureTime, value);
    }

    public TimeSpan TripArrivalTime
    {
        get => _tripArrivalTime;
        set => SetProperty(ref _tripArrivalTime, value);
    }

    public decimal BasePriceMultiplier
    {
        get => _basePriceMultiplier;
        set => SetProperty(ref _basePriceMultiplier, value);
    }

    public bool IsHolidayTrip
    {
        get => _isHolidayTrip;
        set => SetProperty(ref _isHolidayTrip, value);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    #endregion

    #region Commands

    public ICommand AddTripCommand { get; private set; } = null!;
    public ICommand UpdateTripCommand { get; private set; } = null!;
    public ICommand DeleteTripCommand { get; private set; } = null!;
    public ICommand CancelTripCommand { get; private set; } = null!;
    public ICommand SearchTripsCommand { get; private set; } = null!;
    public ICommand LoadTripToFormCommand { get; private set; } = null!;
    public ICommand ShowTodaysTripsCommand { get; private set; } = null!;
    public ICommand ShowActiveTripsCommand { get; private set; } = null!;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        AddTripCommand = new RelayCommand(_ => AddTrip(), _ => CanAddTrip());
        UpdateTripCommand = new RelayCommand(_ => UpdateTrip(), _ => CanUpdateTrip());
        DeleteTripCommand = new RelayCommand(_ => DeleteTrip(), _ => CanDeleteTrip());
        CancelTripCommand = new RelayCommand(_ => CancelTrip(), _ => CanCancelTrip());
        SearchTripsCommand = new RelayCommand(_ => SearchTrips());
        LoadTripToFormCommand = new RelayCommand(_ => LoadTripToForm());
        ShowTodaysTripsCommand = new RelayCommand(_ => ShowTodaysTrips());
        ShowActiveTripsCommand = new RelayCommand(_ => ShowActiveTrips());
    }

    #endregion

    #region Public Methods

    public override void RefreshData()
    {
        try
        {
            SetLoadingState(true, "Loading trips...");
            var trips = _tripService.GetAllTrips();
            Trips = new ObservableCollection<Trip>(trips);
            SetSuccessMessage("Trips loaded successfully");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading trips: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    public override void ClearForm()
    {
        SelectedTrip = null;
        SelectedRoute = null;
        SelectedTrain = null;
        TripDepartureDate = DateTime.Today;
        TripDepartureTime = TimeSpan.FromHours(8);
        TripArrivalTime = TimeSpan.FromHours(12);
        BasePriceMultiplier = 1.0m;
        IsHolidayTrip = false;
        SetStatusMessage("Form cleared");
    }

    public void LoadTripToForm()
    {
        if (SelectedTrip != null)
        {
            TripDepartureDate = SelectedTrip.DepartureDateTime.Date;
            TripDepartureTime = SelectedTrip.DepartureDateTime.TimeOfDay;
            TripArrivalTime = SelectedTrip.ArrivalDateTime.TimeOfDay;
            BasePriceMultiplier = SelectedTrip.BasePriceMultiplier;
            IsHolidayTrip = SelectedTrip.IsHolidayTrip;
            
            // Set selected route and train
            SelectedRoute = Routes.FirstOrDefault(r => r.RouteId == SelectedTrip.RouteId);
            SelectedTrain = Trains.FirstOrDefault(t => t.TrainId == SelectedTrip.TrainId);
        }
    }

    public void ShowTodaysTrips()
    {
        try
        {
            SetLoadingState(true, "Loading today's trips...");
            var trips = _tripService.GetTripsByDateRange(DateTime.Today, DateTime.Today.AddDays(1));
            Trips = new ObservableCollection<Trip>(trips);
            SetSuccessMessage($"Today's trips loaded ({trips.Count()} trips)");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading today's trips: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    public void ShowActiveTrips()
    {
        try
        {
            SetLoadingState(true, "Loading active trips...");
            var trips = _tripService.GetAllTrips().Where(t => t.TripStatus == "Active");
            Trips = new ObservableCollection<Trip>(trips);
            SetSuccessMessage($"Active trips loaded ({trips.Count()} trips)");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading active trips: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    #endregion

    #region Private Methods

    private void LoadTrains()
    {
        try
        {
            var trains = _trainService.GetActiveTrains();
            Trains = new ObservableCollection<Train>(trains);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading trains: {ex.Message}");
        }
    }

    private void LoadRoutes()
    {
        try
        {
            var routes = _routeService.GetAllRoutes();
            Routes = new ObservableCollection<Route>(routes);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading routes: {ex.Message}");
        }
    }

    private bool CanAddTrip()
    {
        return SelectedRoute != null && SelectedTrain != null;
    }

    private void AddTrip()
    {
        if (SelectedRoute == null || SelectedTrain == null) return;

        try
        {
            var departureDateTime = TripDepartureDate.Date + TripDepartureTime;
            var arrivalDateTime = TripDepartureDate.Date + TripArrivalTime;

            var trip = new Trip
            {
                RouteId = SelectedRoute.RouteId,
                TrainId = SelectedTrain.TrainId,
                DepartureDateTime = departureDateTime,
                ArrivalDateTime = arrivalDateTime,
                IsHolidayTrip = IsHolidayTrip,
                TripStatus = "Active",
                BasePriceMultiplier = BasePriceMultiplier
            };

            var createdTrip = _tripService.CreateTrip(trip);
            Trips.Add(createdTrip);
            
            ClearForm();
            SetSuccessMessage("Trip added successfully!");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error adding trip: {ex.Message}");
        }
    }

    private bool CanUpdateTrip()
    {
        return SelectedTrip != null && SelectedRoute != null && SelectedTrain != null;
    }

    private void UpdateTrip()
    {
        if (SelectedTrip == null || SelectedRoute == null || SelectedTrain == null) return;

        try
        {
            var departureDateTime = TripDepartureDate.Date + TripDepartureTime;
            var arrivalDateTime = TripDepartureDate.Date + TripArrivalTime;

            SelectedTrip.RouteId = SelectedRoute.RouteId;
            SelectedTrip.TrainId = SelectedTrain.TrainId;
            SelectedTrip.DepartureDateTime = departureDateTime;
            SelectedTrip.ArrivalDateTime = arrivalDateTime;
            SelectedTrip.IsHolidayTrip = IsHolidayTrip;
            SelectedTrip.BasePriceMultiplier = BasePriceMultiplier;

            var updatedTrip = _tripService.UpdateTrip(SelectedTrip);
            if (updatedTrip != null)
            {
                SetSuccessMessage("Trip updated successfully!");
                RefreshData();
            }
            else
            {
                SetErrorMessage("Failed to update trip!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error updating trip: {ex.Message}");
        }
    }

    private bool CanDeleteTrip()
    {
        return SelectedTrip != null;
    }

    private void DeleteTrip()
    {
        if (SelectedTrip == null) return;

        try
        {
            var success = _tripService.DeleteTrip(SelectedTrip.TripId);
            if (success)
            {
                Trips.Remove(SelectedTrip);
                SetSuccessMessage("Trip deleted successfully!");
                ClearForm();
            }
            else
            {
                SetErrorMessage("Cannot delete trip - it may have bookings!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error deleting trip: {ex.Message}");
        }
    }

    private bool CanCancelTrip()
    {
        return SelectedTrip != null && SelectedTrip.TripStatus == "Active";
    }

    private void CancelTrip()
    {
        if (SelectedTrip == null) return;

        try
        {
            var success = _tripService.CancelTrip(SelectedTrip.TripId);
            if (success)
            {
                SelectedTrip.TripStatus = "Cancelled";
                SetSuccessMessage("Trip cancelled successfully!");
                RefreshData();
            }
            else
            {
                SetErrorMessage("Failed to cancel trip!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error cancelling trip: {ex.Message}");
        }
    }

    private void SearchTrips()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                RefreshData();
                return;
            }

            var trips = _tripService.GetAllTrips()
                .Where(t => t.Train.TrainName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                           t.Route.RouteName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                           t.TripStatus.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            
            Trips = new ObservableCollection<Trip>(trips);
            SetSuccessMessage($"Found {trips.Count()} trips");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error searching trips: {ex.Message}");
        }
    }

    #endregion
}