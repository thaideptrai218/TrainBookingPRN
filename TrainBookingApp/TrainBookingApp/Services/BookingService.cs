using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class BookingService : IBookingService
{
    private readonly Context _context;
    private readonly IPricingRuleService _pricingRuleService;

    public BookingService(Context context, IPricingRuleService pricingRuleService)
    {
        _context = context;
        _pricingRuleService = pricingRuleService;
    }

    public IEnumerable<Booking> GetUserBookings(int userId)
    {
        return _context.Bookings
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Trip)
                .ThenInclude(tr => tr.Train)
                .ThenInclude(train => train.TrainType)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Trip)
                .ThenInclude(tr => tr.Route)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Seat)
                .ThenInclude(s => s.Coach)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Passenger)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDateTime)
            .ToList();
    }

    public IEnumerable<Seat> GetAvailableSeats(int tripId, int? coachId = null)
    {
        var bookedSeatIds = _context.Tickets
            .Where(t => t.TripId == tripId)
            .Select(t => t.SeatId)
            .ToList();

        var heldSeatIds = _context.TemporarySeatHolds
            .Where(h => h.TripId == tripId && h.ExpiresAt > DateTime.Now)
            .Select(h => h.SeatId)
            .ToList();

        var unavailableSeatIds = bookedSeatIds.Concat(heldSeatIds).ToList();

        var query = _context.Seats
            .Include(s => s.Coach)
            .Include(s => s.SeatType)
            .Where(s => !unavailableSeatIds.Contains(s.SeatId));

        if (coachId.HasValue)
        {
            query = query.Where(s => s.CoachId == coachId.Value);
        }

        return query.OrderBy(s => s.Coach.CoachNumber)
                   .ThenBy(s => s.SeatNumber)
                   .ToList();
    }

    public bool IsValidBookingData(Booking booking)
    {
        if (booking == null) return false;
        if (booking.UserId <= 0) return false;
        if (booking.TotalPrice <= 0) return false;
        if (string.IsNullOrWhiteSpace(booking.BookingCode)) return false;
        
        return true;
    }

    public bool CreateBooking(Booking booking, List<Passenger> passengers)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (!IsValidBookingData(booking)) return false;

            // Create booking
            booking.BookingDateTime = DateTime.Now;
            booking.BookingStatus = "Confirmed";
            booking.BookingCode = GenerateBookingReference();
            
            _context.Bookings.Add(booking);
            _context.SaveChanges();

            // Create passengers
            foreach (var passenger in passengers)
            {
                _context.Passengers.Add(passenger);
            }
            _context.SaveChanges();

            // Note: In this simplified version, we're not creating tickets automatically
            // The tickets would be created through a separate seat selection process
            
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    public bool CancelBooking(int bookingId, string reason)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var booking = _context.Bookings
                .Include(b => b.Tickets)
                .FirstOrDefault(b => b.BookingId == bookingId);

            if (booking == null) return false;

            // Update booking status
            booking.BookingStatus = "Cancelled";
            
            // Update ticket status
            foreach (var ticket in booking.Tickets)
            {
                ticket.TicketStatus = "Cancelled";
            }

            _context.SaveChanges();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    public decimal CalculateTotalPrice(int tripId, List<int> seatIds, bool isRoundTrip = false)
    {
        var trip = _context.Trips
            .Include(t => t.Route)
            .Include(t => t.Train)
                .ThenInclude(tr => tr.TrainType)
            .FirstOrDefault(t => t.TripId == tripId);

        if (trip == null) return 0;

        var seats = _context.Seats
            .Include(s => s.SeatType)
            .Where(s => seatIds.Contains(s.SeatId))
            .ToList();

        decimal totalPrice = 0;
        
        foreach (var seat in seats)
        {
            var basePrice = _pricingRuleService.CalculatePrice(
                trip.RouteId, 
                trip.Train.TrainTypeId, 
                isRoundTrip, 
                trip.DepartureDateTime);

            // Apply seat type multiplier
            var seatMultiplier = seat.SeatType?.PriceMultiplier ?? 1.0m;
            totalPrice += basePrice * seatMultiplier;
        }

        return totalPrice;
    }

    public bool HoldSeats(int tripId, List<int> seatIds, int userId, int holdDurationMinutes = 15)
    {
        try
        {
            var expirationTime = DateTime.Now.AddMinutes(holdDurationMinutes);
            
            foreach (var seatId in seatIds)
            {
                var existingHold = _context.TemporarySeatHolds
                    .FirstOrDefault(h => h.TripId == tripId && h.SeatId == seatId);

                if (existingHold != null)
                {
                    existingHold.ExpiresAt = expirationTime;
                    existingHold.UserId = userId;
                }
                else
                {
                    var hold = new TemporarySeatHold
                    {
                        TripId = tripId,
                        SeatId = seatId,
                        UserId = userId,
                        ExpiresAt = expirationTime,
                        CreatedAt = DateTime.Now,
                        SessionId = Guid.NewGuid().ToString(),
                        CoachId = GetCoachIdFromSeat(seatId),
                        LegOriginStationId = 1, // This should be determined from the trip
                        LegDestinationStationId = 1 // This should be determined from the trip
                    };
                    _context.TemporarySeatHolds.Add(hold);
                }
            }

            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ReleaseSeats(int tripId, List<int> seatIds, int userId)
    {
        try
        {
            var holds = _context.TemporarySeatHolds
                .Where(h => seatIds.Contains(h.SeatId) && h.UserId == userId)
                .ToList();

            _context.TemporarySeatHolds.RemoveRange(holds);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<Coach> GetTripCoaches(int tripId)
    {
        var trip = _context.Trips
            .Include(t => t.Train)
                .ThenInclude(tr => tr.Coaches)
                .ThenInclude(c => c.CoachType)
            .FirstOrDefault(t => t.TripId == tripId);

        return trip?.Train?.Coaches?.OrderBy(c => c.CoachNumber).ToList() ?? new List<Coach>();
    }

    public Booking? GetBookingById(int bookingId)
    {
        return _context.Bookings
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Trip)
                .ThenInclude(tr => tr.Train)
                .ThenInclude(train => train.TrainType)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Trip)
                .ThenInclude(tr => tr.Route)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Seat)
                .ThenInclude(s => s.Coach)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Passenger)
            .FirstOrDefault(b => b.BookingId == bookingId);
    }

    public bool ProcessPayment(int bookingId, decimal amount, string paymentMethod)
    {
        // Simplified payment processing - in real app would integrate with payment gateway
        try
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null) return false;

            booking.PaymentStatus = "Paid";
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateBookingReference()
    {
        return $"TRN{DateTime.Now:yyyyMMdd}{DateTime.Now.Ticks % 10000:D4}";
    }

    private decimal CalculateSeatPrice(int seatId)
    {
        var seat = _context.Seats
            .Include(s => s.SeatType)
            .Include(s => s.Coach)
                .ThenInclude(c => c.Train)
                .ThenInclude(t => t.TrainType)
            .FirstOrDefault(s => s.SeatId == seatId);

        if (seat == null) return 0;

        // For now, return a base price - in a real implementation, this would use the pricing rules
        return 50.0m * (seat.SeatType?.PriceMultiplier ?? 1.0m);
    }

    private int GetTripIdFromSeat(int seatId)
    {
        // This is a simplified implementation
        // In a real scenario, you would determine the trip ID from the booking context
        return 1;
    }

    private int GetCoachIdFromSeat(int seatId)
    {
        var seat = _context.Seats.FirstOrDefault(s => s.SeatId == seatId);
        return seat?.CoachId ?? 1;
    }
}