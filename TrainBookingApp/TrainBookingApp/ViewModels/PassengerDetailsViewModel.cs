using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels;

public class PassengerDetailsViewModel : BaseViewModel
{
    private readonly IBookingService _bookingService;
    private readonly List<Seat> _selectedSeats;
    private readonly Trip _selectedTrip;
    private readonly User _currentUser;
    private readonly decimal _totalPrice;
    private readonly bool _isRoundTrip;
    
    private ObservableCollection<PassengerType> _passengerTypes = new();
    private ObservableCollection<PassengerInfo> _passengers = new();
    private string _statusMessage = "Please enter passenger details.";

    public PassengerDetailsViewModel(
        IBookingService bookingService,
        List<Seat> selectedSeats,
        Trip selectedTrip,
        User currentUser,
        decimal totalPrice,
        bool isRoundTrip = false)
    {
        _bookingService = bookingService;
        _selectedSeats = selectedSeats;
        _selectedTrip = selectedTrip;
        _currentUser = currentUser;
        _totalPrice = totalPrice;
        _isRoundTrip = isRoundTrip;
        
        InitializeCommands();
        LoadPassengerTypes();
        InitializePassengers();
    }

    #region Properties

    public List<Seat> SelectedSeats => _selectedSeats;
    public Trip SelectedTrip => _selectedTrip;
    public User CurrentUser => _currentUser;
    public decimal TotalPrice => _totalPrice;
    public bool IsRoundTrip => _isRoundTrip;

    public ObservableCollection<PassengerType> PassengerTypes
    {
        get => _passengerTypes;
        set => SetProperty(ref _passengerTypes, value);
    }

    public ObservableCollection<PassengerInfo> Passengers
    {
        get => _passengers;
        set => SetProperty(ref _passengers, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool CanProceedToPayment => Passengers.All(p => IsPassengerValid(p));

    #endregion

    #region Commands

    public ICommand ProceedToPaymentCommand { get; private set; } = null!;
    public ICommand CancelBookingCommand { get; private set; } = null!;
    public ICommand ValidatePassengerCommand { get; private set; } = null!;

    #endregion

    #region Events

    public event Action<List<PassengerInfo>>? PassengerDetailsCompleted;
    public event Action? BookingCancelled;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        ProceedToPaymentCommand = new RelayCommand(_ => ProceedToPayment(), _ => CanProceedToPayment);
        CancelBookingCommand = new RelayCommand(_ => CancelBooking());
        ValidatePassengerCommand = new RelayCommand(param => ValidatePassenger(param));
    }

    #endregion

    #region Public Methods

    public void LoadPassengerTypes()
    {
        try
        {
            // For now, we'll use a simple hardcoded list
            // In a real application, this would come from the database
            var types = new List<PassengerType>
            {
                new PassengerType { PassengerTypeId = 1, TypeName = "Adult", DiscountPercentage = 0 },
                new PassengerType { PassengerTypeId = 2, TypeName = "Child", DiscountPercentage = 0.5m },
                new PassengerType { PassengerTypeId = 3, TypeName = "Senior", DiscountPercentage = 0.3m },
                new PassengerType { PassengerTypeId = 4, TypeName = "Student", DiscountPercentage = 0.2m }
            };
            
            PassengerTypes = new ObservableCollection<PassengerType>(types);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading passenger types: {ex.Message}";
        }
    }

    public void InitializePassengers()
    {
        try
        {
            var passengers = new List<PassengerInfo>();
            
            for (int i = 0; i < SelectedSeats.Count; i++)
            {
                var passenger = new PassengerInfo
                {
                    Index = i + 1,
                    AssignedSeat = SelectedSeats[i],
                    FullName = string.Empty,
                    IdCardNumber = string.Empty,
                    DateOfBirth = DateTime.Today.AddYears(-30),
                    SelectedPassengerType = PassengerTypes.FirstOrDefault(),
                    IsValid = false
                };
                
                // Set first passenger as current user by default
                if (i == 0)
                {
                    passenger.FullName = CurrentUser.FullName;
                    passenger.IdCardNumber = CurrentUser.IdcardNumber ?? string.Empty;
                    passenger.DateOfBirth = CurrentUser.DateOfBirth?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today.AddYears(-30);
                }
                
                passengers.Add(passenger);
            }
            
            Passengers = new ObservableCollection<PassengerInfo>(passengers);
            StatusMessage = $"Enter details for {Passengers.Count} passengers.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error initializing passengers: {ex.Message}";
        }
    }

    #endregion

    #region Private Methods

    private bool IsPassengerValid(PassengerInfo passenger)
    {
        return !string.IsNullOrWhiteSpace(passenger.FullName) &&
               !string.IsNullOrWhiteSpace(passenger.IdCardNumber) &&
               passenger.DateOfBirth != default &&
               passenger.SelectedPassengerType != null;
    }

    private void ValidatePassenger(object? parameter)
    {
        if (parameter is PassengerInfo passenger)
        {
            passenger.IsValid = IsPassengerValid(passenger);
            OnPropertyChanged(nameof(CanProceedToPayment));
            
            var validCount = Passengers.Count(p => p.IsValid);
            StatusMessage = $"{validCount} of {Passengers.Count} passengers valid.";
        }
    }

    private void ProceedToPayment()
    {
        if (CanProceedToPayment)
        {
            PassengerDetailsCompleted?.Invoke(Passengers.ToList());
        }
    }

    private void CancelBooking()
    {
        try
        {
            // Release held seats
            var seatIds = SelectedSeats.Select(s => s.SeatId).ToList();
            _bookingService.ReleaseSeats(SelectedTrip.TripId, seatIds, CurrentUser.UserId);
            
            BookingCancelled?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error cancelling booking: {ex.Message}";
        }
    }

    #endregion
}

public class PassengerInfo : BaseViewModel
{
    private int _index;
    private Seat _assignedSeat = null!;
    private string _fullName = string.Empty;
    private string _idCardNumber = string.Empty;
    private DateTime _dateOfBirth = DateTime.Today;
    private PassengerType? _selectedPassengerType;
    private bool _isValid = false;

    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    public Seat AssignedSeat
    {
        get => _assignedSeat;
        set => SetProperty(ref _assignedSeat, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string IdCardNumber
    {
        get => _idCardNumber;
        set => SetProperty(ref _idCardNumber, value);
    }

    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set => SetProperty(ref _dateOfBirth, value);
    }

    public PassengerType? SelectedPassengerType
    {
        get => _selectedPassengerType;
        set => SetProperty(ref _selectedPassengerType, value);
    }

    public bool IsValid
    {
        get => _isValid;
        set => SetProperty(ref _isValid, value);
    }
}