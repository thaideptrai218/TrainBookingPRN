using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels;

public class ManagerViewModel : BaseViewModel
{
    private readonly IStationService _stationService;
    private readonly IRouteService _routeService;
    private readonly ITripService _tripService;
    private readonly ITrainService _trainService;
    
    private string _selectedTab = "Stations";
    private int _selectedTabIndex = 0;
    private bool _isLoading = false;
    private string _statusMessage = string.Empty;

    // Station Management Properties
    private ObservableCollection<Station> _stations = new();
    private Station? _selectedStation;
    private string _newStationCode = string.Empty;
    private string _newStationName = string.Empty;
    private string _newStationAddress = string.Empty;
    private string _newStationCity = string.Empty;
    private string _newStationRegion = string.Empty;
    private string _newStationPhone = string.Empty;
    private string _stationSearchTerm = string.Empty;

    // Route Management Properties
    private ObservableCollection<Route> _routes = new();
    private Route? _selectedRoute;
    private string _newRouteName = string.Empty;
    private string _newRouteDescription = string.Empty;
    private ObservableCollection<RouteStation> _routeStations = new();
    private ObservableCollection<Station> _availableStations = new();
    private Station? _selectedAvailableStation;
    private RouteStation? _selectedRouteStation;

    // Trip Management Properties
    private ObservableCollection<Trip> _trips = new();
    private Trip? _selectedTrip;
    private ObservableCollection<Train> _trains = new();
    private Train? _selectedTrain;
    private DateTime _tripDepartureDate = DateTime.Today;
    private TimeSpan _tripDepartureTime = TimeSpan.FromHours(8);
    private TimeSpan _tripArrivalTime = TimeSpan.FromHours(12);
    private decimal _basePriceMultiplier = 1.0m;
    private bool _isHolidayTrip = false;

    public ManagerViewModel(IStationService stationService, IRouteService routeService, 
                          ITripService tripService, ITrainService trainService)
    {
        _stationService = stationService;
        _routeService = routeService;
        _tripService = tripService;
        _trainService = trainService;

        InitializeCommands();
        LoadInitialData();
    }

    #region Properties

    public string SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                LoadTabData();
            }
        }
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (SetProperty(ref _selectedTabIndex, value))
            {
                LoadTabDataByIndex(value);
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    // Station Properties
    public ObservableCollection<Station> Stations
    {
        get => _stations;
        set => SetProperty(ref _stations, value);
    }

    public Station? SelectedStation
    {
        get => _selectedStation;
        set => SetProperty(ref _selectedStation, value);
    }

    public string NewStationCode
    {
        get => _newStationCode;
        set => SetProperty(ref _newStationCode, value);
    }

    public string NewStationName
    {
        get => _newStationName;
        set => SetProperty(ref _newStationName, value);
    }

    public string NewStationAddress
    {
        get => _newStationAddress;
        set => SetProperty(ref _newStationAddress, value);
    }

    public string NewStationCity
    {
        get => _newStationCity;
        set => SetProperty(ref _newStationCity, value);
    }

    public string NewStationRegion
    {
        get => _newStationRegion;
        set => SetProperty(ref _newStationRegion, value);
    }

    public string NewStationPhone
    {
        get => _newStationPhone;
        set => SetProperty(ref _newStationPhone, value);
    }

    public string StationSearchTerm
    {
        get => _stationSearchTerm;
        set => SetProperty(ref _stationSearchTerm, value);
    }

    // Route Properties
    public ObservableCollection<Route> Routes
    {
        get => _routes;
        set => SetProperty(ref _routes, value);
    }

    public Route? SelectedRoute
    {
        get => _selectedRoute;
        set
        {
            if (SetProperty(ref _selectedRoute, value))
            {
                LoadRouteStations();
            }
        }
    }

    public string NewRouteName
    {
        get => _newRouteName;
        set => SetProperty(ref _newRouteName, value);
    }

    public string NewRouteDescription
    {
        get => _newRouteDescription;
        set => SetProperty(ref _newRouteDescription, value);
    }

    public ObservableCollection<RouteStation> RouteStations
    {
        get => _routeStations;
        set => SetProperty(ref _routeStations, value);
    }

    public ObservableCollection<Station> AvailableStations
    {
        get => _availableStations;
        set => SetProperty(ref _availableStations, value);
    }

    public Station? SelectedAvailableStation
    {
        get => _selectedAvailableStation;
        set => SetProperty(ref _selectedAvailableStation, value);
    }

    public RouteStation? SelectedRouteStation
    {
        get => _selectedRouteStation;
        set => SetProperty(ref _selectedRouteStation, value);
    }

    // Trip Properties
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

    #endregion

    #region Commands

    // Station Commands
    public ICommand AddStationCommand { get; private set; } = null!;
    public ICommand UpdateStationCommand { get; private set; } = null!;
    public ICommand DeleteStationCommand { get; private set; } = null!;
    public ICommand SearchStationsCommand { get; private set; } = null!;
    public ICommand ClearStationFormCommand { get; private set; } = null!;

    // Route Commands
    public ICommand AddRouteCommand { get; private set; } = null!;
    public ICommand UpdateRouteCommand { get; private set; } = null!;
    public ICommand DeleteRouteCommand { get; private set; } = null!;
    public ICommand AddStationToRouteCommand { get; private set; } = null!;
    public ICommand RemoveStationFromRouteCommand { get; private set; } = null!;
    public ICommand ClearRouteFormCommand { get; private set; } = null!;

    // Trip Commands
    public ICommand AddTripCommand { get; private set; } = null!;
    public ICommand UpdateTripCommand { get; private set; } = null!;
    public ICommand DeleteTripCommand { get; private set; } = null!;
    public ICommand CancelTripCommand { get; private set; } = null!;
    public ICommand ClearTripFormCommand { get; private set; } = null!;

    // General Commands
    public ICommand RefreshCommand { get; private set; } = null!;
    public ICommand LogoutCommand { get; private set; } = null!;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        // Station Commands
        AddStationCommand = new RelayCommand(_ => AddStation(), _ => CanAddStation());
        UpdateStationCommand = new RelayCommand(_ => UpdateStation(), _ => CanUpdateStation());
        DeleteStationCommand = new RelayCommand(_ => DeleteStation(), _ => CanDeleteStation());
        SearchStationsCommand = new RelayCommand(_ => SearchStations());
        ClearStationFormCommand = new RelayCommand(_ => ClearStationForm());

        // Route Commands
        AddRouteCommand = new RelayCommand(_ => AddRoute(), _ => CanAddRoute());
        UpdateRouteCommand = new RelayCommand(_ => UpdateRoute(), _ => CanUpdateRoute());
        DeleteRouteCommand = new RelayCommand(_ => DeleteRoute(), _ => CanDeleteRoute());
        AddStationToRouteCommand = new RelayCommand(_ => AddStationToRoute(), _ => CanAddStationToRoute());
        RemoveStationFromRouteCommand = new RelayCommand(_ => RemoveStationFromRoute(), _ => CanRemoveStationFromRoute());
        ClearRouteFormCommand = new RelayCommand(_ => ClearRouteForm());

        // Trip Commands
        AddTripCommand = new RelayCommand(_ => AddTrip(), _ => CanAddTrip());
        UpdateTripCommand = new RelayCommand(_ => UpdateTrip(), _ => CanUpdateTrip());
        DeleteTripCommand = new RelayCommand(_ => DeleteTrip(), _ => CanDeleteTrip());
        CancelTripCommand = new RelayCommand(_ => CancelTrip(), _ => CanCancelTrip());
        ClearTripFormCommand = new RelayCommand(_ => ClearTripForm());

        // General Commands
        RefreshCommand = new RelayCommand(_ => RefreshCurrentTab());
        LogoutCommand = new RelayCommand(_ => Logout());
    }

    #endregion

    #region Initialization

    private void LoadInitialData()
    {
        LoadStations();
        LoadRoutes();
        LoadTrips();
        LoadTrains();
    }

    private void LoadTabData()
    {
        switch (SelectedTab)
        {
            case "Stations":
                LoadStations();
                break;
            case "Routes":
                LoadRoutes();
                break;
            case "Trips":
                LoadTrips();
                break;
        }
    }

    private void LoadTabDataByIndex(int index)
    {
        switch (index)
        {
            case 0: // Stations
                LoadStations();
                break;
            case 1: // Routes
                LoadRoutes();
                break;
            case 2: // Trips
                LoadTrips();
                break;
        }
    }

    private void LoadStations()
    {
        try
        {
            var stations = _stationService.GetAllStations();
            Stations = new ObservableCollection<Station>(stations);
            AvailableStations = new ObservableCollection<Station>(stations);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading stations: {ex.Message}";
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
            StatusMessage = $"Error loading routes: {ex.Message}";
        }
    }

    private void LoadTrips()
    {
        try
        {
            var trips = _tripService.GetAllTrips();
            Trips = new ObservableCollection<Trip>(trips);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading trips: {ex.Message}";
        }
    }

    private void LoadTrains()
    {
        try
        {
            var trains = _trainService.GetActiveTrains();
            Trains = new ObservableCollection<Train>(trains);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading trains: {ex.Message}";
        }
    }

    private void LoadRouteStations()
    {
        if (SelectedRoute == null)
        {
            RouteStations.Clear();
            return;
        }

        try
        {
            var routeStations = _routeService.GetRouteStations(SelectedRoute.RouteId);
            RouteStations = new ObservableCollection<RouteStation>(routeStations);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading route stations: {ex.Message}";
        }
    }

    #endregion

    #region Station Management

    private bool CanAddStation()
    {
        return !string.IsNullOrWhiteSpace(NewStationCode) && 
               !string.IsNullOrWhiteSpace(NewStationName);
    }

    private void AddStation()
    {
        try
        {
            if (_stationService.IsStationCodeExists(NewStationCode))
            {
                StatusMessage = "Station code already exists!";
                return;
            }

            var station = new Station
            {
                StationCode = NewStationCode,
                StationName = NewStationName,
                Address = string.IsNullOrWhiteSpace(NewStationAddress) ? null : NewStationAddress,
                City = string.IsNullOrWhiteSpace(NewStationCity) ? null : NewStationCity,
                Region = string.IsNullOrWhiteSpace(NewStationRegion) ? null : NewStationRegion,
                PhoneNumber = string.IsNullOrWhiteSpace(NewStationPhone) ? null : NewStationPhone
            };

            var createdStation = _stationService.CreateStation(station);
            Stations.Add(createdStation);
            AvailableStations.Add(createdStation);
            
            ClearStationForm();
            StatusMessage = "Station added successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding station: {ex.Message}";
        }
    }

    private bool CanUpdateStation()
    {
        return SelectedStation != null && 
               !string.IsNullOrWhiteSpace(NewStationCode) && 
               !string.IsNullOrWhiteSpace(NewStationName);
    }

    private void UpdateStation()
    {
        if (SelectedStation == null) return;

        try
        {
            SelectedStation.StationCode = NewStationCode;
            SelectedStation.StationName = NewStationName;
            SelectedStation.Address = string.IsNullOrWhiteSpace(NewStationAddress) ? null : NewStationAddress;
            SelectedStation.City = string.IsNullOrWhiteSpace(NewStationCity) ? null : NewStationCity;
            SelectedStation.Region = string.IsNullOrWhiteSpace(NewStationRegion) ? null : NewStationRegion;
            SelectedStation.PhoneNumber = string.IsNullOrWhiteSpace(NewStationPhone) ? null : NewStationPhone;

            var updatedStation = _stationService.UpdateStation(SelectedStation);
            if (updatedStation != null)
            {
                StatusMessage = "Station updated successfully!";
                LoadStations(); // Refresh the list
            }
            else
            {
                StatusMessage = "Failed to update station!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating station: {ex.Message}";
        }
    }

    private bool CanDeleteStation()
    {
        return SelectedStation != null;
    }

    private void DeleteStation()
    {
        if (SelectedStation == null) return;

        try
        {
            var success = _stationService.DeleteStation(SelectedStation.StationId);
            if (success)
            {
                Stations.Remove(SelectedStation);
                AvailableStations.Remove(SelectedStation);
                StatusMessage = "Station deleted successfully!";
                ClearStationForm();
            }
            else
            {
                StatusMessage = "Cannot delete station - it may be used in routes!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting station: {ex.Message}";
        }
    }

    private void SearchStations()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(StationSearchTerm))
            {
                LoadStations();
                return;
            }

            var stations = _stationService.SearchStations(StationSearchTerm);
            Stations = new ObservableCollection<Station>(stations);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error searching stations: {ex.Message}";
        }
    }

    private void ClearStationForm()
    {
        NewStationCode = string.Empty;
        NewStationName = string.Empty;
        NewStationAddress = string.Empty;
        NewStationCity = string.Empty;
        NewStationRegion = string.Empty;
        NewStationPhone = string.Empty;
        SelectedStation = null;
    }

    #endregion

    #region Route Management

    private bool CanAddRoute()
    {
        return !string.IsNullOrWhiteSpace(NewRouteName);
    }

    private void AddRoute()
    {
        try
        {
            if (_routeService.IsRouteNameExists(NewRouteName))
            {
                StatusMessage = "Route name already exists!";
                return;
            }

            var route = new Route
            {
                RouteName = NewRouteName,
                Description = string.IsNullOrWhiteSpace(NewRouteDescription) ? null : NewRouteDescription
            };

            var createdRoute = _routeService.CreateRoute(route);
            Routes.Add(createdRoute);
            
            ClearRouteForm();
            StatusMessage = "Route added successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding route: {ex.Message}";
        }
    }

    private bool CanUpdateRoute()
    {
        return SelectedRoute != null && !string.IsNullOrWhiteSpace(NewRouteName);
    }

    private void UpdateRoute()
    {
        if (SelectedRoute == null) return;

        try
        {
            SelectedRoute.RouteName = NewRouteName;
            SelectedRoute.Description = string.IsNullOrWhiteSpace(NewRouteDescription) ? null : NewRouteDescription;

            var updatedRoute = _routeService.UpdateRoute(SelectedRoute);
            if (updatedRoute != null)
            {
                StatusMessage = "Route updated successfully!";
                LoadRoutes(); // Refresh the list
            }
            else
            {
                StatusMessage = "Failed to update route!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating route: {ex.Message}";
        }
    }

    private bool CanDeleteRoute()
    {
        return SelectedRoute != null;
    }

    private void DeleteRoute()
    {
        if (SelectedRoute == null) return;

        try
        {
            var success = _routeService.DeleteRoute(SelectedRoute.RouteId);
            if (success)
            {
                Routes.Remove(SelectedRoute);
                StatusMessage = "Route deleted successfully!";
                ClearRouteForm();
            }
            else
            {
                StatusMessage = "Cannot delete route - it may be used in trips!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting route: {ex.Message}";
        }
    }

    private bool CanAddStationToRoute()
    {
        return SelectedRoute != null && SelectedAvailableStation != null;
    }

    private void AddStationToRoute()
    {
        if (SelectedRoute == null || SelectedAvailableStation == null) return;

        try
        {
            var sequenceNumber = RouteStations.Count + 1;
            var distanceFromStart = sequenceNumber * 10; // Default distance
            var defaultStopTime = 5; // Default 5 minutes stop time

            var success = _routeService.AddStationToRoute(
                SelectedRoute.RouteId,
                SelectedAvailableStation.StationId,
                sequenceNumber,
                distanceFromStart,
                defaultStopTime
            );

            if (success)
            {
                LoadRouteStations();
                StatusMessage = "Station added to route successfully!";
            }
            else
            {
                StatusMessage = "Failed to add station to route - may already exist!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding station to route: {ex.Message}";
        }
    }

    private bool CanRemoveStationFromRoute()
    {
        return SelectedRoute != null && SelectedRouteStation != null;
    }

    private void RemoveStationFromRoute()
    {
        if (SelectedRoute == null || SelectedRouteStation == null) return;

        try
        {
            var success = _routeService.RemoveStationFromRoute(
                SelectedRoute.RouteId,
                SelectedRouteStation.StationId
            );

            if (success)
            {
                LoadRouteStations();
                StatusMessage = "Station removed from route successfully!";
            }
            else
            {
                StatusMessage = "Failed to remove station from route!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error removing station from route: {ex.Message}";
        }
    }

    private void ClearRouteForm()
    {
        NewRouteName = string.Empty;
        NewRouteDescription = string.Empty;
        SelectedRoute = null;
        SelectedAvailableStation = null;
        SelectedRouteStation = null;
    }

    #endregion

    #region Trip Management

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
            
            ClearTripForm();
            StatusMessage = "Trip added successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding trip: {ex.Message}";
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
                StatusMessage = "Trip updated successfully!";
                LoadTrips(); // Refresh the list
            }
            else
            {
                StatusMessage = "Failed to update trip!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating trip: {ex.Message}";
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
                StatusMessage = "Trip deleted successfully!";
                ClearTripForm();
            }
            else
            {
                StatusMessage = "Cannot delete trip - it may have bookings!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting trip: {ex.Message}";
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
                StatusMessage = "Trip cancelled successfully!";
                LoadTrips(); // Refresh the list
            }
            else
            {
                StatusMessage = "Failed to cancel trip!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error cancelling trip: {ex.Message}";
        }
    }

    private void ClearTripForm()
    {
        SelectedTrip = null;
        SelectedRoute = null;
        SelectedTrain = null;
        TripDepartureDate = DateTime.Today;
        TripDepartureTime = TimeSpan.FromHours(8);
        TripArrivalTime = TimeSpan.FromHours(12);
        BasePriceMultiplier = 1.0m;
        IsHolidayTrip = false;
    }

    #endregion

    #region General Methods

    private void RefreshCurrentTab()
    {
        LoadTabData();
        StatusMessage = "Data refreshed successfully!";
    }

    private void Logout()
    {
        // TODO: Implement logout logic
        StatusMessage = "Logout functionality will be implemented!";
    }

    #endregion

    #region Events

    public event Action? LogoutRequested;

    #endregion
}