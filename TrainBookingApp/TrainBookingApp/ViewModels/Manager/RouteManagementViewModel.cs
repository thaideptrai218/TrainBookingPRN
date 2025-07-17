using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class RouteManagementViewModel : BaseManagerViewModel
{
    private readonly IRouteService _routeService;
    private readonly IStationService _stationService;
    
    private ObservableCollection<Route> _routes = new();
    private Route? _selectedRoute;
    private string _newRouteName = string.Empty;
    private string _newRouteDescription = string.Empty;
    private ObservableCollection<RouteStation> _routeStations = new();
    private ObservableCollection<Station> _availableStations = new();
    private Station? _selectedAvailableStation;
    private RouteStation? _selectedRouteStation;
    private string _searchTerm = string.Empty;

    public RouteManagementViewModel(IRouteService routeService, IStationService stationService)
    {
        _routeService = routeService;
        _stationService = stationService;
        InitializeCommands();
        RefreshData();
        LoadAvailableStations();
    }

    public override string TabName => "Route Management";

    #region Properties

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

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    #endregion

    #region Commands

    public ICommand AddRouteCommand { get; private set; } = null!;
    public ICommand UpdateRouteCommand { get; private set; } = null!;
    public ICommand DeleteRouteCommand { get; private set; } = null!;
    public ICommand AddStationToRouteCommand { get; private set; } = null!;
    public ICommand RemoveStationFromRouteCommand { get; private set; } = null!;
    public ICommand SearchRoutesCommand { get; private set; } = null!;
    public ICommand LoadRouteToFormCommand { get; private set; } = null!;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        AddRouteCommand = new RelayCommand(_ => AddRoute(), _ => CanAddRoute());
        UpdateRouteCommand = new RelayCommand(_ => UpdateRoute(), _ => CanUpdateRoute());
        DeleteRouteCommand = new RelayCommand(_ => DeleteRoute(), _ => CanDeleteRoute());
        AddStationToRouteCommand = new RelayCommand(_ => AddStationToRoute(), _ => CanAddStationToRoute());
        RemoveStationFromRouteCommand = new RelayCommand(_ => RemoveStationFromRoute(), _ => CanRemoveStationFromRoute());
        SearchRoutesCommand = new RelayCommand(_ => SearchRoutes());
        LoadRouteToFormCommand = new RelayCommand(_ => LoadRouteToForm());
    }

    #endregion

    #region Public Methods

    public override void RefreshData()
    {
        try
        {
            SetLoadingState(true, "Loading routes...");
            var routes = _routeService.GetAllRoutes();
            Routes = new ObservableCollection<Route>(routes);
            SetSuccessMessage("Routes loaded successfully");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading routes: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    public override void ClearForm()
    {
        NewRouteName = string.Empty;
        NewRouteDescription = string.Empty;
        SelectedRoute = null;
        SelectedAvailableStation = null;
        SelectedRouteStation = null;
        SetStatusMessage("Form cleared");
    }

    public void LoadRouteToForm()
    {
        if (SelectedRoute != null)
        {
            NewRouteName = SelectedRoute.RouteName;
            NewRouteDescription = SelectedRoute.Description ?? string.Empty;
        }
    }

    #endregion

    #region Private Methods

    private void LoadAvailableStations()
    {
        try
        {
            var stations = _stationService.GetAllStations();
            AvailableStations = new ObservableCollection<Station>(stations);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading available stations: {ex.Message}");
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
            SetErrorMessage($"Error loading route stations: {ex.Message}");
        }
    }

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
                SetErrorMessage("Route name already exists!");
                return;
            }

            var route = new Route
            {
                RouteName = NewRouteName,
                Description = string.IsNullOrWhiteSpace(NewRouteDescription) ? null : NewRouteDescription
            };

            var createdRoute = _routeService.CreateRoute(route);
            Routes.Add(createdRoute);
            
            ClearForm();
            SetSuccessMessage("Route added successfully!");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error adding route: {ex.Message}");
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
                SetSuccessMessage("Route updated successfully!");
                RefreshData();
            }
            else
            {
                SetErrorMessage("Failed to update route!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error updating route: {ex.Message}");
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
                SetSuccessMessage("Route deleted successfully!");
                ClearForm();
            }
            else
            {
                SetErrorMessage("Cannot delete route - it may be used in trips!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error deleting route: {ex.Message}");
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
                SetSuccessMessage("Station added to route successfully!");
            }
            else
            {
                SetErrorMessage("Failed to add station to route - may already exist!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error adding station to route: {ex.Message}");
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
                SetSuccessMessage("Station removed from route successfully!");
            }
            else
            {
                SetErrorMessage("Failed to remove station from route!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error removing station from route: {ex.Message}");
        }
    }

    private void SearchRoutes()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                RefreshData();
                return;
            }

            var routes = _routeService.GetAllRoutes()
                .Where(r => r.RouteName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                           (r.Description != null && r.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            
            Routes = new ObservableCollection<Route>(routes);
            SetSuccessMessage($"Found {routes.Count()} routes");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error searching routes: {ex.Message}");
        }
    }

    #endregion
}