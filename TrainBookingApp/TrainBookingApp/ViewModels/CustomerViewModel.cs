using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;
using TrainBookingApp.Views;

namespace TrainBookingApp.ViewModels;

public class CustomerViewModel : BaseViewModel
{
    private readonly IBookingService _bookingService;
    private readonly ITripService _tripService;
    private readonly IStationService _stationService;
    private readonly IPricingRuleService _pricingRuleService;
    private User? _currentUser;
    
    private ObservableCollection<Trip> _availableTrips = new();
    private ObservableCollection<Station> _stations = new();
    private ObservableCollection<Booking> _userBookings = new();
    
    private Station? _fromStation;
    private Station? _toStation;
    private DateTime _departureDate = DateTime.Today;
    private DateTime? _returnDate;
    private bool _isRoundTrip = false;
    private int _passengerCount = 1;
    private Trip? _selectedTrip;
    private string _statusMessage = "Welcome! Search for trips to get started.";

    public CustomerViewModel(
        IBookingService bookingService,
        ITripService tripService,
        IStationService stationService,
        IPricingRuleService pricingRuleService)
    {
        _bookingService = bookingService;
        _tripService = tripService;
        _stationService = stationService;
        _pricingRuleService = pricingRuleService;
        
        InitializeCommands();
    }

    public void Initialize(User currentUser)
    {
        _currentUser = currentUser;
        LoadInitialData();
    }

    #region Properties

    public User? CurrentUser => _currentUser;

    public ObservableCollection<Trip> AvailableTrips
    {
        get => _availableTrips;
        set => SetProperty(ref _availableTrips, value);
    }

    public ObservableCollection<Station> Stations
    {
        get => _stations;
        set => SetProperty(ref _stations, value);
    }

    public ObservableCollection<Booking> UserBookings
    {
        get => _userBookings;
        set => SetProperty(ref _userBookings, value);
    }

    public Station? FromStation
    {
        get => _fromStation;
        set => SetProperty(ref _fromStation, value);
    }

    public Station? ToStation
    {
        get => _toStation;
        set => SetProperty(ref _toStation, value);
    }

    public DateTime DepartureDate
    {
        get => _departureDate;
        set => SetProperty(ref _departureDate, value);
    }

    public DateTime? ReturnDate
    {
        get => _returnDate;
        set => SetProperty(ref _returnDate, value);
    }

    public bool IsRoundTrip
    {
        get => _isRoundTrip;
        set
        {
            SetProperty(ref _isRoundTrip, value);
            if (!value)
            {
                ReturnDate = null;
            }
        }
    }

    public int PassengerCount
    {
        get => _passengerCount;
        set => SetProperty(ref _passengerCount, Math.Max(1, Math.Min(9, value)));
    }

    public Trip? SelectedTrip
    {
        get => _selectedTrip;
        set => SetProperty(ref _selectedTrip, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    #endregion

    #region Commands

    public ICommand SearchTripsCommand { get; private set; } = null!;
    public ICommand BookTripCommand { get; private set; } = null!;
    public ICommand ViewBookingHistoryCommand { get; private set; } = null!;
    public ICommand CancelBookingCommand { get; private set; } = null!;
    public ICommand SwapStationsCommand { get; private set; } = null!;
    public ICommand RefreshDataCommand { get; private set; } = null!;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        SearchTripsCommand = new RelayCommand(_ => SearchTrips(), _ => CanSearchTrips());
        BookTripCommand = new RelayCommand(_ => BookTrip(), _ => CanBookTrip());
        ViewBookingHistoryCommand = new RelayCommand(_ => ViewBookingHistory());
        CancelBookingCommand = new RelayCommand(param => CancelBooking(param), param => CanCancelBooking(param));
        SwapStationsCommand = new RelayCommand(_ => SwapStations(), _ => CanSwapStations());
        RefreshDataCommand = new RelayCommand(_ => RefreshData());
    }

    #endregion

    #region Public Methods

    public void LoadInitialData()
    {
        try
        {
            var stations = _stationService.GetAllStations();
            Stations = new ObservableCollection<Station>(stations);
            
            LoadUserBookings();
            StatusMessage = $"Welcome, {CurrentUser?.FullName}! Ready to search for trips.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
        }
    }

    public void LoadUserBookings()
    {
        try
        {
            if (CurrentUser != null)
            {
                var bookings = _bookingService.GetUserBookings(CurrentUser.UserId);
                UserBookings = new ObservableCollection<Booking>(bookings);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading bookings: {ex.Message}";
        }
    }

    public void RefreshData()
    {
        LoadInitialData();
        if (FromStation != null && ToStation != null)
        {
            SearchTrips();
        }
    }

    #endregion

    #region Private Methods

    private bool CanSearchTrips()
    {
        return FromStation != null && 
               ToStation != null && 
               FromStation.StationId != ToStation.StationId &&
               DepartureDate >= DateTime.Today &&
               PassengerCount > 0 &&
               (!IsRoundTrip || (ReturnDate.HasValue && ReturnDate.Value > DepartureDate));
    }

    private void SearchTrips()
    {
        if (!CanSearchTrips()) return;

        try
        {
            StatusMessage = "Searching for trips...";
            
            var trips = _tripService.SearchTrips(
                FromStation!.StationId, 
                ToStation!.StationId, 
                DepartureDate);

            AvailableTrips = new ObservableCollection<Trip>(trips);
            
            if (AvailableTrips.Any())
            {
                StatusMessage = $"Found {AvailableTrips.Count} available trips";
            }
            else
            {
                StatusMessage = "No trips found for your search criteria";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error searching trips: {ex.Message}";
        }
    }

    private bool CanBookTrip()
    {
        return SelectedTrip != null && PassengerCount > 0;
    }

    private void BookTrip()
    {
        if (!CanBookTrip()) return;

        try
        {
            StatusMessage = $"Starting booking process for trip {SelectedTrip!.TripId} with {PassengerCount} passengers...";
            
            // Start the booking workflow
            StartBookingWorkflow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error starting booking: {ex.Message}";
        }
    }

    private void StartBookingWorkflow()
    {
        if (SelectedTrip == null || CurrentUser == null) return;

        try
        {
            // Step 1: Seat Selection
            var seatSelectionViewModel = new SeatSelectionViewModel(
                _bookingService,
                _pricingRuleService,
                SelectedTrip,
                PassengerCount,
                CurrentUser,
                IsRoundTrip);

            var seatSelectionWindow = new SeatSelectionWindow(seatSelectionViewModel);
            var seatResult = seatSelectionWindow.ShowDialog();

            if (seatResult == true && seatSelectionViewModel.SelectedSeats.Any())
            {
                // Step 2: Passenger Details
                var passengerDetailsViewModel = new PassengerDetailsViewModel(
                    _bookingService,
                    seatSelectionViewModel.SelectedSeats.ToList(),
                    SelectedTrip,
                    CurrentUser,
                    seatSelectionViewModel.TotalPrice,
                    IsRoundTrip);

                var passengerDetailsWindow = new PassengerDetailsWindow(passengerDetailsViewModel);
                var passengerResult = passengerDetailsWindow.ShowDialog();

                if (passengerResult == true)
                {
                    // Step 3: Booking Confirmation
                    var bookingConfirmationViewModel = new BookingConfirmationViewModel(
                        _bookingService,
                        SelectedTrip,
                        seatSelectionViewModel.SelectedSeats.ToList(),
                        passengerDetailsViewModel.Passengers.ToList(),
                        CurrentUser,
                        seatSelectionViewModel.TotalPrice,
                        IsRoundTrip);

                    var bookingConfirmationWindow = new BookingConfirmationWindow(bookingConfirmationViewModel);
                    var confirmationResult = bookingConfirmationWindow.ShowDialog();

                    if (confirmationResult == true)
                    {
                        StatusMessage = "Booking completed successfully!";
                        LoadUserBookings(); // Refresh booking history
                    }
                    else
                    {
                        StatusMessage = "Booking cancelled.";
                    }
                }
                else
                {
                    StatusMessage = "Passenger details cancelled.";
                }
            }
            else
            {
                StatusMessage = "Seat selection cancelled.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error in booking workflow: {ex.Message}";
        }
    }

    private void ViewBookingHistory()
    {
        try
        {
            LoadUserBookings();
            StatusMessage = $"Loaded {UserBookings.Count} bookings";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading booking history: {ex.Message}";
        }
    }

    private bool CanCancelBooking(object? parameter)
    {
        return parameter is Booking booking && 
               booking.BookingStatus == "Confirmed" &&
               (booking.ExpiredAt == null || booking.ExpiredAt > DateTime.Now);
    }

    private void CancelBooking(object? parameter)
    {
        if (parameter is not Booking booking) return;

        try
        {
            var success = _bookingService.CancelBooking(booking.BookingId, "Customer cancellation");
            if (success)
            {
                LoadUserBookings();
                StatusMessage = $"Booking {booking.BookingCode} cancelled successfully";
            }
            else
            {
                StatusMessage = "Failed to cancel booking";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error cancelling booking: {ex.Message}";
        }
    }

    private bool CanSwapStations()
    {
        return FromStation != null && ToStation != null;
    }

    private void SwapStations()
    {
        if (!CanSwapStations()) return;

        var temp = FromStation;
        FromStation = ToStation;
        ToStation = temp;
        
        StatusMessage = "Stations swapped";
    }

    #endregion
}