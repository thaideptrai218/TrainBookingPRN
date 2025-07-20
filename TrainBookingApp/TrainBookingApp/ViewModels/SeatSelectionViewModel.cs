using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels;

public class SeatSelectionViewModel : BaseViewModel
{
    private readonly IBookingService _bookingService;
    private readonly IPricingRuleService _pricingRuleService;
    private readonly Trip _selectedTrip;
    private readonly int _passengerCount;
    private readonly User _currentUser;
    
    private ObservableCollection<Coach> _coaches = new();
    private ObservableCollection<Seat> _availableSeats = new();
    private ObservableCollection<Seat> _selectedSeats = new();
    private Coach? _selectedCoach;
    private decimal _totalPrice = 0;
    private bool _isRoundTrip = false;
    private string _statusMessage = "Please select seats for your passengers.";

    public SeatSelectionViewModel(
        IBookingService bookingService,
        IPricingRuleService pricingRuleService,
        Trip selectedTrip,
        int passengerCount,
        User currentUser,
        bool isRoundTrip = false)
    {
        _bookingService = bookingService;
        _pricingRuleService = pricingRuleService;
        _selectedTrip = selectedTrip;
        _passengerCount = passengerCount;
        _currentUser = currentUser;
        _isRoundTrip = isRoundTrip;
        
        InitializeCommands();
        LoadCoaches();
    }

    #region Properties

    public Trip SelectedTrip => _selectedTrip;
    public int PassengerCount => _passengerCount;
    public User CurrentUser => _currentUser;
    public bool IsRoundTrip => _isRoundTrip;

    public ObservableCollection<Coach> Coaches
    {
        get => _coaches;
        set => SetProperty(ref _coaches, value);
    }

    public ObservableCollection<Seat> AvailableSeats
    {
        get => _availableSeats;
        set => SetProperty(ref _availableSeats, value);
    }

    public ObservableCollection<Seat> SelectedSeats
    {
        get => _selectedSeats;
        set => SetProperty(ref _selectedSeats, value);
    }

    public Coach? SelectedCoach
    {
        get => _selectedCoach;
        set
        {
            SetProperty(ref _selectedCoach, value);
            if (value != null)
            {
                LoadSeatsForCoach(value.CoachId);
            }
        }
    }

    public decimal TotalPrice
    {
        get => _totalPrice;
        set => SetProperty(ref _totalPrice, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool CanProceedToBooking => SelectedSeats.Count == PassengerCount;

    #endregion

    #region Commands

    public ICommand SelectSeatCommand { get; private set; } = null!;
    public ICommand UnselectSeatCommand { get; private set; } = null!;
    public ICommand ProceedToBookingCommand { get; private set; } = null!;
    public ICommand CancelSelectionCommand { get; private set; } = null!;
    public ICommand RefreshSeatsCommand { get; private set; } = null!;

    #endregion

    #region Events

    public event Action<List<Seat>>? SeatSelectionCompleted;
    public event Action? SelectionCancelled;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        SelectSeatCommand = new RelayCommand(param => SelectSeat(param), param => CanSelectSeat(param));
        UnselectSeatCommand = new RelayCommand(param => UnselectSeat(param), param => CanUnselectSeat(param));
        ProceedToBookingCommand = new RelayCommand(_ => ProceedToBooking(), _ => CanProceedToBooking);
        CancelSelectionCommand = new RelayCommand(_ => CancelSelection());
        RefreshSeatsCommand = new RelayCommand(_ => RefreshSeats());
    }

    #endregion

    #region Public Methods

    public void LoadCoaches()
    {
        try
        {
            var coaches = _bookingService.GetTripCoaches(SelectedTrip.TripId);
            Coaches = new ObservableCollection<Coach>(coaches);
            
            if (Coaches.Any())
            {
                SelectedCoach = Coaches.First();
                StatusMessage = $"Loaded {Coaches.Count} coaches. Select {PassengerCount} seats.";
            }
            else
            {
                StatusMessage = "No coaches available for this trip.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading coaches: {ex.Message}";
        }
    }

    public void LoadSeatsForCoach(int coachId)
    {
        try
        {
            var seats = _bookingService.GetAvailableSeats(SelectedTrip.TripId, coachId);
            AvailableSeats = new ObservableCollection<Seat>(seats);
            
            StatusMessage = $"Loaded {AvailableSeats.Count} available seats in coach {SelectedCoach?.CoachName}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading seats: {ex.Message}";
        }
    }

    public void RefreshSeats()
    {
        if (SelectedCoach != null)
        {
            LoadSeatsForCoach(SelectedCoach.CoachId);
        }
    }

    #endregion

    #region Private Methods

    private bool CanSelectSeat(object? parameter)
    {
        return parameter is Seat seat && 
               SelectedSeats.Count < PassengerCount && 
               !SelectedSeats.Contains(seat);
    }

    private void SelectSeat(object? parameter)
    {
        if (parameter is not Seat seat) return;

        try
        {
            // Hold the seat temporarily
            var success = _bookingService.HoldSeats(SelectedTrip.TripId, new List<int> { seat.SeatId }, CurrentUser.UserId);
            
            if (success)
            {
                SelectedSeats.Add(seat);
                AvailableSeats.Remove(seat);
                UpdateTotalPrice();
                
                StatusMessage = $"Seat {seat.SeatName} selected. {PassengerCount - SelectedSeats.Count} more needed.";
                OnPropertyChanged(nameof(CanProceedToBooking));
            }
            else
            {
                StatusMessage = "Failed to reserve seat. Please try another seat.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error selecting seat: {ex.Message}";
        }
    }

    private bool CanUnselectSeat(object? parameter)
    {
        return parameter is Seat seat && SelectedSeats.Contains(seat);
    }

    private void UnselectSeat(object? parameter)
    {
        if (parameter is not Seat seat) return;

        try
        {
            // Release the seat hold
            var success = _bookingService.ReleaseSeats(SelectedTrip.TripId, new List<int> { seat.SeatId }, CurrentUser.UserId);
            
            if (success)
            {
                SelectedSeats.Remove(seat);
                AvailableSeats.Add(seat);
                UpdateTotalPrice();
                
                StatusMessage = $"Seat {seat.SeatName} unselected. {PassengerCount - SelectedSeats.Count} more needed.";
                OnPropertyChanged(nameof(CanProceedToBooking));
            }
            else
            {
                StatusMessage = "Failed to release seat reservation.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error unselecting seat: {ex.Message}";
        }
    }

    private void ProceedToBooking()
    {
        if (SelectedSeats.Count == PassengerCount)
        {
            SeatSelectionCompleted?.Invoke(SelectedSeats.ToList());
        }
    }

    private void CancelSelection()
    {
        try
        {
            // Release all held seats
            var seatIds = SelectedSeats.Select(s => s.SeatId).ToList();
            if (seatIds.Any())
            {
                _bookingService.ReleaseSeats(SelectedTrip.TripId, seatIds, CurrentUser.UserId);
            }
            
            SelectionCancelled?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error cancelling selection: {ex.Message}";
        }
    }

    private void UpdateTotalPrice()
    {
        try
        {
            if (SelectedSeats.Any())
            {
                var seatIds = SelectedSeats.Select(s => s.SeatId).ToList();
                TotalPrice = _bookingService.CalculateTotalPrice(SelectedTrip.TripId, seatIds, IsRoundTrip);
            }
            else
            {
                TotalPrice = 0;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error calculating price: {ex.Message}";
        }
    }

    #endregion
}