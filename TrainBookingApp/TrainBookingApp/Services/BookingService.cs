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

    public IEnumerable<SeatWithStatus> GetAllSeatsWithStatus(int tripId, int? coachId = null)
    {
        var bookedSeatIds = _context.Tickets
            .Where(t => t.TripId == tripId)
            .Select(t => t.SeatId)
            .ToList();

        var heldSeatIds = _context.TemporarySeatHolds
            .Where(h => h.TripId == tripId && h.ExpiresAt > DateTime.Now)
            .Select(h => h.SeatId)
            .ToList();

        var query = _context.Seats
            .Include(s => s.Coach)
            .Include(s => s.SeatType)
            .AsQueryable();

        if (coachId.HasValue)
        {
            query = query.Where(s => s.CoachId == coachId.Value);
        }

        var allSeats = query.OrderBy(s => s.Coach.CoachNumber)
                           .ThenBy(s => s.SeatNumber)
                           .ToList();

        return allSeats.Select(seat => new SeatWithStatus
        {
            Seat = seat,
            Status = bookedSeatIds.Contains(seat.SeatId) ? SeatStatus.Occupied :
                    heldSeatIds.Contains(seat.SeatId) ? SeatStatus.Held :
                    SeatStatus.Available,
            IsSelected = false
        }).ToList();
    }

    public bool IsValidBookingData(Booking booking)
    {
        if (booking == null) return false;
        if (booking.UserId <= 0) return false;
        if (booking.TotalPrice <= 0) return false;
        if (string.IsNullOrWhiteSpace(booking.BookingCode)) return false;
        
        return true;
    }

    public bool CreateBooking(Booking booking, List<Passenger> passengers, int tripId, List<int> seatIds)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (!IsValidBookingData(booking)) return false;
            
            if (passengers.Count != seatIds.Count)
            {
                throw new ArgumentException("Number of passengers must match number of seats");
            }

            // Create booking
            booking.BookingDateTime = DateTime.Now;
            booking.BookingStatus = "Confirmed";
            booking.BookingCode = GenerateBookingReference();
            
            _context.Bookings.Add(booking);
            _context.SaveChanges();

            // Create passengers and get their IDs
            var createdPassengers = new List<Passenger>();
            foreach (var passenger in passengers)
            {
                _context.Passengers.Add(passenger);
                _context.SaveChanges(); // Save to get PassengerId
                createdPassengers.Add(passenger);
            }

            // Create tickets for each passenger-seat combination
            for (int i = 0; i < createdPassengers.Count; i++)
            {
                var passenger = createdPassengers[i];
                var seatId = seatIds[i];
                
                // Get seat details for price calculation
                var seat = _context.Seats
                    .Include(s => s.SeatType)
                    .Include(s => s.Coach)
                        .ThenInclude(c => c.CoachType)
                    .FirstOrDefault(s => s.SeatId == seatId);
                
                if (seat == null)
                {
                    throw new InvalidOperationException($"Seat with ID {seatId} not found");
                }

                // Calculate individual ticket price
                var ticketPrice = CalculateTicketPrice(tripId, seatId);
                
                // Get trip details for start/end stations
                var trip = _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.RouteStations)
                        .ThenInclude(rs => rs.Station)
                    .FirstOrDefault(t => t.TripId == tripId);
                
                var startStation = trip?.Route?.RouteStations?.OrderBy(rs => rs.SequenceNumber)?.First()?.Station;
                var endStation = trip?.Route?.RouteStations?.OrderByDescending(rs => rs.SequenceNumber)?.First()?.Station;
                
                var ticket = new Ticket
                {
                    BookingId = booking.BookingId,
                    TripId = tripId,
                    SeatId = seatId,
                    PassengerId = passenger.PassengerId,
                    StartStationId = startStation?.StationId ?? 1,
                    EndStationId = endStation?.StationId ?? 1,
                    Price = ticketPrice,
                    TicketStatus = "Valid",
                    TicketCode = GenerateTicketCode(booking.BookingCode, i + 1),
                    CoachNameSnapshot = seat.Coach?.CoachType?.TypeName ?? "Unknown",
                    SeatNameSnapshot = seat.SeatName ?? $"Seat {seat.SeatNumber}",
                    PassengerNameSnapshot = passenger.FullName,
                    PassengerIdcardNumberSnapshot = passenger.IdcardNumber,
                    IsRefundable = true
                };
                
                _context.Tickets.Add(ticket);
            }
            _context.SaveChanges();

            // Release any temporary seat holds for these seats
            var holds = _context.TemporarySeatHolds
                .Where(h => h.TripId == tripId && seatIds.Contains(h.SeatId) && h.UserId == booking.UserId)
                .ToList();
            
            if (holds.Any())
            {
                _context.TemporarySeatHolds.RemoveRange(holds);
                _context.SaveChanges();
            }
            
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
                ticket.TicketStatus = "Refunded";
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

    public bool HoldSeats(int tripId, List<int> seatIds, int userId, int holdDurationMinutes = 5)
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

    private decimal CalculateTicketPrice(int tripId, int seatId)
    {
        try
        {
            var trip = _context.Trips
                .Include(t => t.Route)
                .Include(t => t.Train)
                    .ThenInclude(tr => tr.TrainType)
                .FirstOrDefault(t => t.TripId == tripId);

            var seat = _context.Seats
                .Include(s => s.SeatType)
                .FirstOrDefault(s => s.SeatId == seatId);

            if (trip == null || seat == null) return 0;

            // Get base price from pricing rules
            var basePrice = _pricingRuleService.CalculatePrice(
                trip.RouteId,
                trip.Train.TrainTypeId,
                false, // Individual ticket price, not round trip
                trip.DepartureDateTime);

            // Apply seat type multiplier
            var seatMultiplier = seat.SeatType?.PriceMultiplier ?? 1.0m;
            
            // Apply trip multiplier (for holiday trips, etc.)
            var tripMultiplier = trip.BasePriceMultiplier;

            return basePrice * seatMultiplier * tripMultiplier;
        }
        catch
        {
            // Fallback to basic calculation
            return CalculateSeatPrice(seatId);
        }
    }

    private string GenerateTicketCode(string bookingCode, int sequenceNumber)
    {
        return $"{bookingCode}-T{sequenceNumber:D2}";
    }

    public IEnumerable<Ticket> GetBookingTickets(int bookingId)
    {
        try
        {
            return _context.Tickets
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Train)
                    .ThenInclude(train => train.TrainType)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                .Include(t => t.Seat)
                    .ThenInclude(s => s.Coach)
                    .ThenInclude(c => c.CoachType)
                .Include(t => t.Seat)
                    .ThenInclude(s => s.SeatType)
                .Include(t => t.Passenger)
                    .ThenInclude(p => p.PassengerType)
                .Include(t => t.StartStation)
                .Include(t => t.EndStation)
                .Where(t => t.BookingId == bookingId)
                .OrderBy(t => t.TicketCode)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving tickets for booking {bookingId}: {ex.Message}", ex);
        }
    }

    public Booking? GetBookingWithTicketsAndDetails(int bookingId)
    {
        try
        {
            return _context.Bookings
                .Include(b => b.User)
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
                    .ThenInclude(c => c.CoachType)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Seat)
                    .ThenInclude(s => s.SeatType)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Passenger)
                    .ThenInclude(p => p.PassengerType)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.StartStation)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.EndStation)
                .FirstOrDefault(b => b.BookingId == bookingId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving detailed booking {bookingId}: {ex.Message}", ex);
        }
    }
}