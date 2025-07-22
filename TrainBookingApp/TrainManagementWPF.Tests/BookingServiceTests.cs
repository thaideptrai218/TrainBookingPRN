using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainManagementWPF.Tests;

// Test-specific DbContext that only uses in-memory database
public class TestContext : Context
{
    public TestContext(DbContextOptions<Context> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Do not call base.OnConfiguring to avoid SQL Server configuration
        // The options are already set via constructor
    }
}

public class BookingServiceTests : IDisposable
{
    private readonly TestContext _context;
    private readonly BookingService _bookingService;
    private readonly IPricingRuleService _pricingRuleService;

    public BookingServiceTests()
    {
        // Setup in-memory database with unique name for each test instance
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new TestContext(options);

        // Create a mock pricing rule service
        _pricingRuleService = new MockPricingRuleService();

        // Initialize the BookingService with dependencies
        _bookingService = new BookingService(_context, _pricingRuleService);

        // Seed test data
        SeedTestData();
    }

    [Fact]
    public void CreateBooking_ShouldSaveBookingToDatabase()
    {
        // Arrange
        var booking = new Booking
        {
            UserId = 1,
            TotalPrice = 150.00m,
            BookingCode = "TEST001",
            BookingDateTime = DateTime.Now,
            BookingStatus = "Pending",
            PaymentStatus = "Unpaid",
            Source = "Web"
        };

        var passengers = new List<Passenger>
        {
            new Passenger
            {
                UserId = 1,
                FullName = "John Doe",
                IdcardNumber = "123456789",
                PassengerTypeId = 1
            }
        };

        var seatIds = new List<int> { 2 }; // Use seat 2 instead of 1 (which is already booked)

        // Act
        var result = _bookingService.CreateBooking(booking, passengers, 1, seatIds);

        // Assert
        Assert.True(result);
        
        var savedBooking = _context.Bookings
            .Include(b => b.Tickets)
            .FirstOrDefault(b => b.UserId == 1);
        
        Assert.NotNull(savedBooking);
        Assert.Equal("Confirmed", savedBooking.BookingStatus);
        Assert.Single(savedBooking.Tickets);
        Assert.Contains("TRN", savedBooking.BookingCode);
    }

    [Fact]
    public void IsValidBookingData_WithValidBooking_ShouldReturnTrue()
    {
        // Arrange
        var validBooking = new Booking
        {
            UserId = 1,
            TotalPrice = 100.00m,
            BookingCode = "TEST123"
        };

        // Act
        var result = _bookingService.IsValidBookingData(validBooking);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidBookingData_WithInvalidBooking_ShouldReturnFalse()
    {
        // Arrange - Test null booking
        Booking? nullBooking = null;

        // Act & Assert
        Assert.False(_bookingService.IsValidBookingData(nullBooking!));

        // Arrange - Test booking with invalid user ID
        var invalidUserBooking = new Booking
        {
            UserId = 0,
            TotalPrice = 100.00m,
            BookingCode = "TEST123"
        };

        // Act & Assert
        Assert.False(_bookingService.IsValidBookingData(invalidUserBooking));

        // Arrange - Test booking with invalid price
        var invalidPriceBooking = new Booking
        {
            UserId = 1,
            TotalPrice = 0,
            BookingCode = "TEST123"
        };

        // Act & Assert
        Assert.False(_bookingService.IsValidBookingData(invalidPriceBooking));
    }

    [Fact]
    public void GetUserBookings_WithExistingBookings_ShouldReturnBookings()
    {
        // Arrange
        var userId = 1;

        // Act
        var bookings = _bookingService.GetUserBookings(userId);

        // Assert
        Assert.NotNull(bookings);
        Assert.Single(bookings);
        Assert.Equal(userId, bookings.First().UserId);
    }

    [Fact]
    public void GetBookingById_WithExistingId_ShouldReturnBooking()
    {
        // Arrange
        var bookingId = 1;

        // Act
        var booking = _bookingService.GetBookingById(bookingId);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(bookingId, booking.BookingId);
    }

    [Fact]
    public void GetBookingById_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var booking = _bookingService.GetBookingById(nonExistentId);

        // Assert
        Assert.Null(booking);
    }

    [Fact]
    public void CancelBooking_WithExistingBooking_ShouldCancelSuccessfully()
    {
        // Arrange
        var bookingId = 1;
        var reason = "Change of plans";

        // Act
        var result = _bookingService.CancelBooking(bookingId, reason);

        // Assert
        Assert.True(result);
        
        var cancelledBooking = _context.Bookings
            .Include(b => b.Tickets)
            .FirstOrDefault(b => b.BookingId == bookingId);
        
        Assert.NotNull(cancelledBooking);
        Assert.Equal("Cancelled", cancelledBooking.BookingStatus);
        Assert.All(cancelledBooking.Tickets, ticket => 
            Assert.Equal("Refunded", ticket.TicketStatus));
    }

    [Fact]
    public void ProcessPayment_WithValidBooking_ShouldUpdatePaymentStatus()
    {
        // Arrange
        var bookingId = 1;
        var amount = 150.00m;
        var paymentMethod = "Credit Card";

        // Act
        var result = _bookingService.ProcessPayment(bookingId, amount, paymentMethod);

        // Assert
        Assert.True(result);
        
        var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
        Assert.NotNull(booking);
        Assert.Equal("Paid", booking.PaymentStatus);
    }

    [Fact]
    public void GetAvailableSeats_WithValidTrip_ShouldReturnAvailableSeats()
    {
        // Arrange
        var tripId = 1;

        // Act
        var availableSeats = _bookingService.GetAvailableSeats(tripId);

        // Assert
        Assert.NotNull(availableSeats);
        Assert.NotEmpty(availableSeats);
    }

    [Fact]
    public void HoldSeats_WithValidData_ShouldCreateSeatHolds()
    {
        // Arrange
        var tripId = 1;
        var seatIds = new List<int> { 2, 3 };
        var userId = 2;

        // Act
        var result = _bookingService.HoldSeats(tripId, seatIds, userId, 10);

        // Assert
        Assert.True(result);
        
        var holds = _context.TemporarySeatHolds
            .Where(h => h.UserId == userId && seatIds.Contains(h.SeatId))
            .ToList();
        
        Assert.Equal(2, holds.Count);
        Assert.All(holds, hold => Assert.True(hold.ExpiresAt > DateTime.Now));
    }

    [Fact]
    public void ReleaseSeats_WithExistingHolds_ShouldRemoveHolds()
    {
        // Arrange - First create some holds
        var tripId = 1;
        var seatIds = new List<int> { 2, 3 };
        var userId = 2;
        _bookingService.HoldSeats(tripId, seatIds, userId);

        // Act
        var result = _bookingService.ReleaseSeats(tripId, seatIds, userId);

        // Assert
        Assert.True(result);
        
        var remainingHolds = _context.TemporarySeatHolds
            .Where(h => h.UserId == userId && seatIds.Contains(h.SeatId))
            .ToList();
        
        Assert.Empty(remainingHolds);
    }

    private void SeedTestData()
    {
        // Seed Users
        _context.Users.AddRange(
            new User 
            { 
                UserId = 1, 
                Email = "test1@example.com", 
                FullName = "Test User 1", 
                Role = "Customer",
                PhoneNumber = "123456789",
                PasswordHash = "hashedpassword",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new User 
            { 
                UserId = 2, 
                Email = "test2@example.com", 
                FullName = "Test User 2", 
                Role = "Customer",
                PhoneNumber = "987654321",
                PasswordHash = "hashedpassword",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        );

        // Seed PassengerTypes
        _context.PassengerTypes.Add(
            new PassengerType { PassengerTypeId = 1, TypeName = "Adult", DiscountPercentage = 0.0m }
        );

        // Seed TrainTypes
        _context.TrainTypes.Add(
            new TrainType { TrainTypeId = 1, TypeName = "Express", AverageVelocity = 120.0m }
        );

        // Seed Trains
        _context.Trains.Add(
            new Train { TrainId = 1, TrainName = "Express 101", TrainTypeId = 1 }
        );

        // Seed Routes
        _context.Routes.Add(
            new Route { RouteId = 1, RouteName = "City A - City B" }
        );

        // Seed Stations
        _context.Stations.AddRange(
            new Station { StationId = 1, StationName = "Central Station", StationCode = "CS001", City = "City A" },
            new Station { StationId = 2, StationName = "North Station", StationCode = "NS002", City = "City B" }
        );

        // Seed RouteStations
        _context.RouteStations.AddRange(
            new RouteStation { RouteStationId = 1, RouteId = 1, StationId = 1, SequenceNumber = 1, DistanceFromStart = 0.0m, DefaultStopTime = 5 },
            new RouteStation { RouteStationId = 2, RouteId = 1, StationId = 2, SequenceNumber = 2, DistanceFromStart = 100.0m, DefaultStopTime = 5 }
        );

        // Seed Trips
        _context.Trips.Add(
            new Trip 
            { 
                TripId = 1, 
                TrainId = 1, 
                RouteId = 1,
                DepartureDateTime = DateTime.Now.AddDays(1),
                ArrivalDateTime = DateTime.Now.AddDays(1).AddHours(3),
                BasePriceMultiplier = 1.0m,
                TripStatus = "Scheduled"
            }
        );

        // Seed TripStations
        _context.TripStations.AddRange(
            new TripStation { TripStationId = 1, TripId = 1, StationId = 1, SequenceNumber = 1, ScheduledArrival = DateTime.Now.AddDays(1), ScheduledDeparture = DateTime.Now.AddDays(1).AddMinutes(5) },
            new TripStation { TripStationId = 2, TripId = 1, StationId = 2, SequenceNumber = 2, ScheduledArrival = DateTime.Now.AddDays(1).AddHours(3), ScheduledDeparture = DateTime.Now.AddDays(1).AddHours(3) }
        );

        // Seed CoachTypes
        _context.CoachTypes.Add(
            new CoachType 
            { 
                CoachTypeId = 1, 
                TypeName = "Economy", 
                PriceMultiplier = 1.0m,
                Description = "Standard economy class",
                IsCompartmented = false,
                DefaultCompartmentCapacity = 4
            }
        );

        // Seed Coaches
        _context.Coaches.Add(
            new Coach 
            { 
                CoachId = 1, 
                TrainId = 1, 
                CoachTypeId = 1, 
                CoachNumber = 1,
                CoachName = "EC-1",
                PositionInTrain = 1
            }
        );

        // Seed SeatTypes
        _context.SeatTypes.Add(
            new SeatType { SeatTypeId = 1, TypeName = "Standard", PriceMultiplier = 1.0m }
        );

        // Seed Seats
        _context.Seats.AddRange(
            new Seat { SeatId = 1, CoachId = 1, SeatTypeId = 1, SeatNumber = 1, SeatName = "1A" },
            new Seat { SeatId = 2, CoachId = 1, SeatTypeId = 1, SeatNumber = 2, SeatName = "1B" },
            new Seat { SeatId = 3, CoachId = 1, SeatTypeId = 1, SeatNumber = 3, SeatName = "1C" }
        );

        // Seed existing Bookings for testing
        var existingBooking = new Booking
        {
            BookingId = 1,
            UserId = 1,
            TotalPrice = 150.00m,
            BookingCode = "TRN202501010001",
            BookingDateTime = DateTime.Now.AddDays(-1),
            BookingStatus = "Confirmed",
            PaymentStatus = "Paid",
            Source = "Web"
        };
        _context.Bookings.Add(existingBooking);

        // Seed existing Tickets
        var existingTicket = new Ticket
        {
            TicketId = 1,
            BookingId = 1,
            TripId = 1,
            SeatId = 1,
            PassengerId = 1,
            StartStationId = 1,
            EndStationId = 2,
            Price = 150.00m,
            TicketStatus = "Valid",
            TicketCode = "TRN202501010001-T01",
            CoachNameSnapshot = "Economy",
            SeatNameSnapshot = "1A",
            PassengerNameSnapshot = "Test Passenger",
            PassengerIdcardNumberSnapshot = "123456789",
            IsRefundable = true
        };
        _context.Tickets.Add(existingTicket);

        // Seed existing Passengers
        var existingPassenger = new Passenger
        {
            PassengerId = 1,
            UserId = 1,
            FullName = "Test Passenger",
            IdcardNumber = "123456789",
            PassengerTypeId = 1
        };
        _context.Passengers.Add(existingPassenger);

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

// Mock implementation for testing
public class MockPricingRuleService : IPricingRuleService
{
    private readonly List<PricingRule> _mockRules = new();

    public MockPricingRuleService()
    {
        _mockRules.Add(new PricingRule
        {
            RuleId = 1,
            RouteId = 1,
            TrainTypeId = 1,
            BasePricePerKm = 0.5m,
            RuleName = "Mock Rule",
            IsActive = true
        });
    }

    public decimal CalculatePrice(int routeId, int trainTypeId, bool isRoundTrip, DateTime travelDate)
    {
        // Simple mock calculation
        return isRoundTrip ? 200.00m : 100.00m;
    }

    public List<PricingRule> GetAllPricingRules()
    {
        return _mockRules.ToList();
    }

    public PricingRule? GetPricingRuleById(int ruleId)
    {
        return _mockRules.FirstOrDefault(r => r.RuleId == ruleId);
    }

    public List<PricingRule> GetActivePricingRules()
    {
        return _mockRules.Where(r => r.IsActive).ToList();
    }

    public List<PricingRule> GetPricingRulesByTrainType(int trainTypeId)
    {
        return _mockRules.Where(r => r.TrainTypeId == trainTypeId).ToList();
    }

    public List<PricingRule> GetPricingRulesByRoute(int routeId)
    {
        return _mockRules.Where(r => r.RouteId == routeId).ToList();
    }

    public bool AddPricingRule(PricingRule rule)
    {
        rule.RuleId = _mockRules.Count + 1;
        _mockRules.Add(rule);
        return true;
    }

    public bool UpdatePricingRule(PricingRule rule)
    {
        var existing = _mockRules.FirstOrDefault(r => r.RuleId == rule.RuleId);
        if (existing != null)
        {
            _mockRules.Remove(existing);
            _mockRules.Add(rule);
            return true;
        }
        return false;
    }

    public bool DeletePricingRule(int ruleId)
    {
        var rule = _mockRules.FirstOrDefault(r => r.RuleId == ruleId);
        if (rule != null)
        {
            _mockRules.Remove(rule);
            return true;
        }
        return false;
    }

    public bool DeactivatePricingRule(int ruleId)
    {
        var rule = _mockRules.FirstOrDefault(r => r.RuleId == ruleId);
        if (rule != null)
        {
            rule.IsActive = false;
            return true;
        }
        return false;
    }

    public bool ActivatePricingRule(int ruleId)
    {
        var rule = _mockRules.FirstOrDefault(r => r.RuleId == ruleId);
        if (rule != null)
        {
            rule.IsActive = true;
            return true;
        }
        return false;
    }
}