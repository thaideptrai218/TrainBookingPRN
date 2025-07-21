using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels;

public class BookingConfirmationViewModel : BaseViewModel
{
    private readonly IBookingService _bookingService;
    private readonly IPassengerTypeService _passengerTypeService;
    private readonly Trip _selectedTrip;
    private readonly List<Seat> _selectedSeats;
    private readonly List<PassengerInfo> _passengers;
    private readonly User _currentUser;
    private readonly decimal _totalPrice;
    private readonly bool _isRoundTrip;

    private string _selectedPaymentMethod = "Credit Card";
    private string _statusMessage = "Review your booking details and proceed with payment.";
    private bool _isProcessing = false;

    public BookingConfirmationViewModel(
        IBookingService bookingService,
        IPassengerTypeService passengerTypeService,
        Trip selectedTrip,
        List<Seat> selectedSeats,
        List<PassengerInfo> passengers,
        User currentUser,
        decimal totalPrice,
        bool isRoundTrip = false)
    {
        _bookingService = bookingService;
        _passengerTypeService = passengerTypeService;
        _selectedTrip = selectedTrip;
        _selectedSeats = selectedSeats;
        _passengers = passengers;
        _currentUser = currentUser;
        _totalPrice = totalPrice;
        _isRoundTrip = isRoundTrip;

        InitializeCommands();
    }

    #region Properties

    public Trip SelectedTrip => _selectedTrip;
    public List<Seat> SelectedSeats => _selectedSeats;
    public List<PassengerInfo> Passengers => _passengers;
    public User CurrentUser => _currentUser;
    public decimal TotalPrice => _totalPrice;
    public bool IsRoundTrip => _isRoundTrip;

    public List<string> PaymentMethods => new List<string>
    {
        "Credit Card",
        "Debit Card",
        "Bank Transfer",
        "Digital Wallet"
    };

    public string SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set => SetProperty(ref _selectedPaymentMethod, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetProperty(ref _isProcessing, value);
    }

    public bool CanConfirmBooking => !IsProcessing && !string.IsNullOrWhiteSpace(SelectedPaymentMethod);

    #endregion

    #region Commands

    public ICommand ConfirmBookingCommand { get; private set; } = null!;
    public ICommand CancelBookingCommand { get; private set; } = null!;

    #endregion

    #region Events

    public event Action<Booking>? BookingCompleted;
    public event Action? BookingCancelled;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        ConfirmBookingCommand = new RelayCommand(_ => ConfirmBooking(), _ => CanConfirmBooking);
        CancelBookingCommand = new RelayCommand(_ => CancelBooking());
    }

    #endregion

    #region Private Methods

    private async void ConfirmBooking()
    {
        if (!CanConfirmBooking) return;

        IsProcessing = true;
        StatusMessage = "Processing your booking...";

        try
        {
            // Create booking
            var booking = new Booking
            {
                UserId = CurrentUser.UserId,
                BookingDateTime = DateTime.Now,
                TotalPrice = TotalPrice,
                BookingStatus = "Pending",
                PaymentStatus = "Unpaid",
                BookingCode = GenerateBookingCode(),
                Source = "Web Application"
            };

            // Create passengers
            var passengers = new List<Passenger>();
            for (int i = 0; i < Passengers.Count; i++)
            {
                var passengerInfo = Passengers[i];
                // Get default passenger type if none selected
                int passengerTypeId = passengerInfo.SelectedPassengerType?.PassengerTypeId ?? GetDefaultPassengerTypeId();
                
                var passenger = new Passenger
                {
                    FullName = passengerInfo.FullName,
                    IdcardNumber = passengerInfo.IdCardNumber,
                    DateOfBirth = DateOnly.FromDateTime(passengerInfo.DateOfBirth),
                    PassengerTypeId = passengerTypeId,
                    UserId = CurrentUser.UserId
                };
                passengers.Add(passenger);
            }

            // Create booking with passengers and tickets
            var seatIds = SelectedSeats.Select(s => s.SeatId).ToList();
            var bookingSuccess = _bookingService.CreateBooking(booking, passengers, SelectedTrip.TripId, seatIds);

            if (bookingSuccess)
            {
                StatusMessage = "Booking created successfully. Processing payment...";

                // Simulate payment processing
                await Task.Delay(2000); // Simulate payment processing time

                // Process payment
                var paymentSuccess = _bookingService.ProcessPayment(booking.BookingId, TotalPrice, SelectedPaymentMethod);

                if (paymentSuccess)
                {
                    StatusMessage = "Payment processed successfully! Tickets have been issued.";
                    BookingCompleted?.Invoke(booking);
                }
                else
                {
                    StatusMessage = "Payment failed. Please try again.";

                    // Cancel the booking if payment failed
                    _bookingService.CancelBooking(booking.BookingId, "Payment failed");
                }
            }
            else
            {
                StatusMessage = "Failed to create booking. Please try again.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error processing booking: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            OnPropertyChanged(nameof(CanConfirmBooking));
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

    private string GenerateBookingCode()
    {
        return $"TRN{DateTime.Now:yyyyMMdd}{DateTime.Now.Ticks % 10000:D4}";
    }

    private int GetDefaultPassengerTypeId()
    {
        try
        {
            var passengerTypes = _passengerTypeService.GetAllPassengerTypes();
            var defaultType = passengerTypes.FirstOrDefault(pt => pt.TypeName.Equals("Adult", StringComparison.OrdinalIgnoreCase))
                           ?? passengerTypes.FirstOrDefault();
            
            if (defaultType == null)
            {
                throw new InvalidOperationException("No passenger types found in database. Please ensure passenger types are configured.");
            }
            
            return defaultType.PassengerTypeId;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unable to get default passenger type: {ex.Message}", ex);
        }
    }

    #endregion
}