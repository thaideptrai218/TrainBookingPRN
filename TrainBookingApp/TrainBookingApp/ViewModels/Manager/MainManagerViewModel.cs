using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;

namespace TrainBookingApp.ViewModels.Manager;

public class MainManagerViewModel : BaseViewModel
{
    private int _selectedTabIndex = 0;
    private bool _isLoading = false;
    private string _statusMessage = string.Empty;
    private User? _currentUser;

    public MainManagerViewModel(
        StationManagementViewModel stationManagementViewModel,
        TrainTypeManagementViewModel trainTypeManagementViewModel,
        TrainManagementViewModel trainManagementViewModel,
        RouteManagementViewModel routeManagementViewModel,
        TripManagementViewModel tripManagementViewModel,
        PricingRuleManagementViewModel pricingRuleManagementViewModel)
    {
        StationManagementViewModel = stationManagementViewModel;
        TrainTypeManagementViewModel = trainTypeManagementViewModel;
        TrainManagementViewModel = trainManagementViewModel;
        RouteManagementViewModel = routeManagementViewModel;
        TripManagementViewModel = tripManagementViewModel;
        PricingRuleManagementViewModel = pricingRuleManagementViewModel;

        // Initialize tab ViewModels collection
        TabViewModels = new ObservableCollection<IManagerTabViewModel>
        {
            StationManagementViewModel,
            TrainTypeManagementViewModel,
            TrainManagementViewModel,
            RouteManagementViewModel,
            TripManagementViewModel,
            PricingRuleManagementViewModel
        };

        InitializeCommands();
        
        // Subscribe to status messages from all tabs
        SubscribeToTabStatusMessages();
    }

    #region Properties

    public ObservableCollection<IManagerTabViewModel> TabViewModels { get; }

    public StationManagementViewModel StationManagementViewModel { get; }
    public TrainTypeManagementViewModel TrainTypeManagementViewModel { get; }
    public TrainManagementViewModel TrainManagementViewModel { get; }
    public RouteManagementViewModel RouteManagementViewModel { get; }
    public TripManagementViewModel TripManagementViewModel { get; }
    public PricingRuleManagementViewModel PricingRuleManagementViewModel { get; }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (SetProperty(ref _selectedTabIndex, value))
            {
                OnTabChanged();
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

    public User? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public string WelcomeMessage => CurrentUser != null ? $"Welcome, {CurrentUser.FullName}" : "Manager Dashboard";

    #endregion

    #region Commands

    public ICommand RefreshAllCommand { get; private set; } = null!;
    public ICommand RefreshCurrentTabCommand { get; private set; } = null!;
    public ICommand LogoutCommand { get; private set; } = null!;
    public ICommand SwitchToStationsCommand { get; private set; } = null!;
    public ICommand SwitchToTrainTypesCommand { get; private set; } = null!;
    public ICommand SwitchToTrainsCommand { get; private set; } = null!;
    public ICommand SwitchToRoutesCommand { get; private set; } = null!;
    public ICommand SwitchToTripsCommand { get; private set; } = null!;
    public ICommand SwitchToPricingRulesCommand { get; private set; } = null!;

    #endregion

    #region Events

    public event Action? LogoutRequested;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        RefreshAllCommand = new RelayCommand(_ => RefreshAll());
        RefreshCurrentTabCommand = new RelayCommand(_ => RefreshCurrentTab());
        LogoutCommand = new RelayCommand(_ => Logout());
        SwitchToStationsCommand = new RelayCommand(_ => SwitchToTab(0));
        SwitchToTrainTypesCommand = new RelayCommand(_ => SwitchToTab(1));
        SwitchToTrainsCommand = new RelayCommand(_ => SwitchToTab(2));
        SwitchToRoutesCommand = new RelayCommand(_ => SwitchToTab(3));
        SwitchToTripsCommand = new RelayCommand(_ => SwitchToTab(4));
        SwitchToPricingRulesCommand = new RelayCommand(_ => SwitchToTab(5));
    }

    #endregion

    #region Public Methods

    public void Initialize(User user)
    {
        CurrentUser = user;
        StatusMessage = "Manager dashboard initialized successfully";
    }

    public void RefreshAll()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Refreshing all data...";

            foreach (var tabViewModel in TabViewModels)
            {
                tabViewModel.RefreshData();
            }

            StatusMessage = "All data refreshed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void RefreshCurrentTab()
    {
        try
        {
            if (SelectedTabIndex >= 0 && SelectedTabIndex < TabViewModels.Count)
            {
                var currentTab = TabViewModels[SelectedTabIndex];
                currentTab.RefreshData();
                StatusMessage = $"{currentTab.TabName} data refreshed";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing current tab: {ex.Message}";
        }
    }

    public void ClearAllForms()
    {
        foreach (var tabViewModel in TabViewModels)
        {
            tabViewModel.ClearForm();
        }
        StatusMessage = "All forms cleared";
    }

    public void SwitchToTab(int tabIndex)
    {
        if (tabIndex >= 0 && tabIndex < TabViewModels.Count)
        {
            SelectedTabIndex = tabIndex;
        }
    }

    #endregion

    #region Private Methods

    private void OnTabChanged()
    {
        if (SelectedTabIndex >= 0 && SelectedTabIndex < TabViewModels.Count)
        {
            var currentTab = TabViewModels[SelectedTabIndex];
            StatusMessage = $"Switched to {currentTab.TabName}";
        }
    }

    private void SubscribeToTabStatusMessages()
    {
        // Subscribe to property changes of all tab ViewModels to get status updates
        foreach (var tabViewModel in TabViewModels)
        {
            if (tabViewModel is BaseManagerViewModel baseViewModel)
            {
                baseViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(BaseManagerViewModel.StatusMessage))
                    {
                        // Forward the status message from the active tab
                        if (sender == TabViewModels[SelectedTabIndex])
                        {
                            StatusMessage = baseViewModel.StatusMessage;
                        }
                    }
                    else if (args.PropertyName == nameof(BaseManagerViewModel.IsLoading))
                    {
                        // Forward the loading state from the active tab
                        if (sender == TabViewModels[SelectedTabIndex])
                        {
                            IsLoading = baseViewModel.IsLoading;
                        }
                    }
                };
            }
        }
    }

    private void Logout()
    {
        try
        {
            // Clear all forms before logout
            ClearAllForms();
            
            // Clear current user
            CurrentUser = null;
            
            StatusMessage = "Logged out successfully";
            
            // Notify the view to handle logout
            LogoutRequested?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during logout: {ex.Message}";
        }
    }

    #endregion

    #region Statistics Methods

    public void GetDashboardStatistics()
    {
        try
        {
            var stationCount = StationManagementViewModel.Stations.Count;
            var trainTypeCount = TrainTypeManagementViewModel.TrainTypes.Count;
            var trainCount = TrainManagementViewModel.Trains.Count;
            var routeCount = RouteManagementViewModel.Routes.Count;
            var tripCount = TripManagementViewModel.Trips.Count;
            var activeTrips = TripManagementViewModel.Trips.Count(t => t.TripStatus == "Active");

            StatusMessage = $"Dashboard: {stationCount} stations, {trainTypeCount} train types, " +
                           $"{trainCount} trains, {routeCount} routes, {tripCount} trips ({activeTrips} active)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error getting statistics: {ex.Message}";
        }
    }

    #endregion
}