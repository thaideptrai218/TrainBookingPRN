using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels;

public class BookingDetailsViewModel : BaseViewModel
{
    private readonly IBookingService _bookingService;
    private Booking? _booking;
    private ObservableCollection<TicketDetailsInfo> _tickets = new();
    private string _statusMessage = "Loading booking details...";

    public BookingDetailsViewModel(IBookingService bookingService, int bookingId)
    {
        _bookingService = bookingService;
        
        InitializeCommands();
        LoadBookingDetails(bookingId);
    }

    #region Properties

    public Booking? Booking
    {
        get => _booking;
        set => SetProperty(ref _booking, value);
    }

    public ObservableCollection<TicketDetailsInfo> Tickets
    {
        get => _tickets;
        set => SetProperty(ref _tickets, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    // Booking Summary Properties
    public string BookingCode => Booking?.BookingCode ?? "N/A";
    public DateTime BookingDate => Booking?.BookingDateTime ?? DateTime.MinValue;
    public string BookingStatus => Booking?.BookingStatus ?? "Unknown";
    public string PaymentStatus => Booking?.PaymentStatus ?? "Unknown";
    public decimal TotalPrice => Booking?.TotalPrice ?? 0;
    public int TicketCount => Tickets?.Count ?? 0;

    // Trip Summary Properties (from first ticket)
    public string RouteName => Tickets?.FirstOrDefault()?.RouteName ?? "N/A";
    public string TrainInfo => Tickets?.FirstOrDefault()?.TrainInfo ?? "N/A";
    public DateTime DepartureTime => Tickets?.FirstOrDefault()?.DepartureTime ?? DateTime.MinValue;
    public DateTime ArrivalTime => Tickets?.FirstOrDefault()?.ArrivalTime ?? DateTime.MinValue;

    #endregion

    #region Commands

    public ICommand CloseCommand { get; private set; } = null!;
    public ICommand PrintTicketsCommand { get; private set; } = null!;
    public ICommand RefreshCommand { get; private set; } = null!;

    #endregion

    #region Events

    public event Action? RequestClose;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        CloseCommand = new RelayCommand(_ => Close());
        PrintTicketsCommand = new RelayCommand(_ => PrintTickets(), _ => CanPrintTickets());
        RefreshCommand = new RelayCommand(_ => RefreshBookingDetails());
    }

    #endregion

    #region Private Methods

    private void LoadBookingDetails(int bookingId)
    {
        try
        {
            StatusMessage = "Loading booking details...";
            
            var bookingWithDetails = _bookingService.GetBookingWithTicketsAndDetails(bookingId);
            
            if (bookingWithDetails == null)
            {
                StatusMessage = "Booking not found.";
                return;
            }

            Booking = bookingWithDetails;
            
            // Convert tickets to display format
            var ticketDetailsList = new List<TicketDetailsInfo>();
            
            foreach (var ticket in bookingWithDetails.Tickets.OrderBy(t => t.TicketCode))
            {
                var ticketInfo = new TicketDetailsInfo
                {
                    TicketCode = ticket.TicketCode,
                    PassengerName = ticket.PassengerNameSnapshot,
                    PassengerIdCard = ticket.PassengerIdcardNumberSnapshot ?? "N/A",
                    PassengerType = ticket.Passenger?.PassengerType?.TypeName ?? "N/A",
                    SeatInfo = $"{ticket.CoachNameSnapshot} - {ticket.SeatNameSnapshot}",
                    SeatType = ticket.Seat?.SeatType?.TypeName ?? "N/A",
                    Price = ticket.Price,
                    Status = ticket.TicketStatus,
                    IsRefundable = ticket.IsRefundable ?? false,
                    
                    // Trip details
                    RouteName = ticket.Trip?.Route?.RouteName ?? "N/A",
                    TrainInfo = $"{ticket.Trip?.Train?.TrainName ?? "N/A"} - {ticket.Trip?.Train?.TrainType?.TypeName ?? "N/A"}",
                    DepartureTime = ticket.Trip?.DepartureDateTime ?? DateTime.MinValue,
                    ArrivalTime = ticket.Trip?.ArrivalDateTime ?? DateTime.MinValue,
                    StartStation = ticket.StartStation?.StationName ?? "N/A",
                    EndStation = ticket.EndStation?.StationName ?? "N/A"
                };
                
                ticketDetailsList.Add(ticketInfo);
            }
            
            Tickets = new ObservableCollection<TicketDetailsInfo>(ticketDetailsList);
            StatusMessage = $"Loaded {Tickets.Count} tickets successfully.";
            
            // Notify property changes for computed properties
            OnPropertyChanged(nameof(BookingCode));
            OnPropertyChanged(nameof(BookingDate));
            OnPropertyChanged(nameof(BookingStatus));
            OnPropertyChanged(nameof(PaymentStatus));
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TicketCount));
            OnPropertyChanged(nameof(RouteName));
            OnPropertyChanged(nameof(TrainInfo));
            OnPropertyChanged(nameof(DepartureTime));
            OnPropertyChanged(nameof(ArrivalTime));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading booking details: {ex.Message}";
        }
    }

    private void RefreshBookingDetails()
    {
        if (Booking != null)
        {
            LoadBookingDetails(Booking.BookingId);
        }
    }

    private bool CanPrintTickets()
    {
        return Tickets?.Any() == true;
    }

    private void PrintTickets()
    {
        // TODO: Implement print functionality
        StatusMessage = "Print functionality will be implemented in future version.";
    }

    private void Close()
    {
        RequestClose?.Invoke();
    }

    #endregion
}

public class TicketDetailsInfo : BaseViewModel
{
    public string TicketCode { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerIdCard { get; set; } = string.Empty;
    public string PassengerType { get; set; } = string.Empty;
    public string SeatInfo { get; set; } = string.Empty;
    public string SeatType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsRefundable { get; set; }
    
    // Trip Information
    public string RouteName { get; set; } = string.Empty;
    public string TrainInfo { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string StartStation { get; set; } = string.Empty;
    public string EndStation { get; set; } = string.Empty;
    
    // Computed Properties
    public string FormattedPrice => Price.ToString("C");
    public string FormattedDepartureTime => DepartureTime.ToString("MM/dd HH:mm");
    public string FormattedArrivalTime => ArrivalTime.ToString("MM/dd HH:mm");
    public string StatusColor => Status switch
    {
        "Valid" => "#28a745",
        "Used" => "#6c757d", 
        "Cancelled" => "#dc3545",
        "Refunded" => "#ffc107",
        _ => "#6c757d"
    };
}