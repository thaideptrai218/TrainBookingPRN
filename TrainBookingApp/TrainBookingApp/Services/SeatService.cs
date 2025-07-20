using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class SeatService : ISeatService
{
    private readonly Context _context;

    public SeatService(Context context)
    {
        _context = context;
    }

    public IEnumerable<Seat> GetAllSeats()
    {
        return _context.Seats
            .Include(s => s.Coach)
            .ThenInclude(c => c.Train)
            .Include(s => s.SeatType)
            .OrderBy(s => s.Coach.Train.TrainName)
            .ThenBy(s => s.Coach.PositionInTrain)
            .ThenBy(s => s.SeatNumber)
            .ToList();
    }

    public Seat? GetSeatById(int seatId)
    {
        return _context.Seats
            .Include(s => s.Coach)
            .ThenInclude(c => c.Train)
            .Include(s => s.SeatType)
            .FirstOrDefault(s => s.SeatId == seatId);
    }

    public IEnumerable<Seat> GetEnabledSeats()
    {
        return _context.Seats
            .Include(s => s.Coach)
            .ThenInclude(c => c.Train)
            .Include(s => s.SeatType)
            .Where(s => s.IsEnabled)
            .OrderBy(s => s.Coach.Train.TrainName)
            .ThenBy(s => s.Coach.PositionInTrain)
            .ThenBy(s => s.SeatNumber)
            .ToList();
    }

    public IEnumerable<Seat> GetSeatsByCoachId(int coachId)
    {
        return _context.Seats
            .Include(s => s.Coach)
            .ThenInclude(c => c.Train)
            .Include(s => s.SeatType)
            .Where(s => s.CoachId == coachId)
            .OrderBy(s => s.SeatNumber)
            .ToList();
    }

    public IEnumerable<Seat> GetEnabledSeatsByCoachId(int coachId)
    {
        return _context.Seats
            .Include(s => s.Coach)
            .ThenInclude(c => c.Train)
            .Include(s => s.SeatType)
            .Where(s => s.CoachId == coachId && s.IsEnabled)
            .OrderBy(s => s.SeatNumber)
            .ToList();
    }

    public IEnumerable<Seat> GetSeatsByTrainId(int trainId)
    {
        return _context.Seats
            .Include(s => s.Coach)
            .ThenInclude(c => c.Train)
            .Include(s => s.SeatType)
            .Where(s => s.Coach.TrainId == trainId)
            .OrderBy(s => s.Coach.PositionInTrain)
            .ThenBy(s => s.SeatNumber)
            .ToList();
    }

    public IEnumerable<Seat> GetAvailableSeatsForTrip(int tripId, int coachId)
    {
        return _context.Seats
            .Include(s => s.Coach)
            .ThenInclude(c => c.Train)
            .Include(s => s.SeatType)
            .Where(s => s.CoachId == coachId && s.IsEnabled)
            .Where(s => !s.Tickets.Any(t => t.TripId == tripId))
            .Where(s => !s.TemporarySeatHolds.Any(h => h.TripId == tripId && h.ExpiresAt > DateTime.Now))
            .OrderBy(s => s.SeatNumber)
            .ToList();
    }

    public Seat CreateSeat(Seat seat)
    {
        try
        {
            _context.Seats.Add(seat);
            _context.SaveChanges();
            return seat;
        }
        catch
        {
            throw;
        }
    }

    public Seat? UpdateSeat(Seat seat)
    {
        try
        {
            _context.Seats.Update(seat);
            _context.SaveChanges();
            return seat;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteSeat(int seatId)
    {
        try
        {
            var seat = _context.Seats.Find(seatId);
            if (seat == null) return false;

            _context.Seats.Remove(seat);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool EnableSeat(int seatId)
    {
        try
        {
            var seat = _context.Seats.Find(seatId);
            if (seat == null) return false;

            seat.IsEnabled = true;
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DisableSeat(int seatId)
    {
        try
        {
            var seat = _context.Seats.Find(seatId);
            if (seat == null) return false;

            seat.IsEnabled = false;
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsSeatNumberUniqueInCoach(int coachId, int seatNumber, int? excludeSeatId = null)
    {
        try
        {
            var query = _context.Seats
                .Where(s => s.CoachId == coachId && s.SeatNumber == seatNumber);

            if (excludeSeatId.HasValue)
            {
                query = query.Where(s => s.SeatId != excludeSeatId.Value);
            }

            return !query.Any();
        }
        catch
        {
            return false;
        }
    }

    public bool IsSeatAvailableForTrip(int seatId, int tripId)
    {
        try
        {
            var seat = _context.Seats
                .Include(s => s.Tickets)
                .Include(s => s.TemporarySeatHolds)
                .FirstOrDefault(s => s.SeatId == seatId);

            if (seat == null || !seat.IsEnabled)
                return false;

            // Check if seat is already booked for this trip
            if (seat.Tickets.Any(t => t.TripId == tripId))
                return false;

            // Check if seat is currently held for this trip
            if (seat.TemporarySeatHolds.Any(h => h.TripId == tripId && h.ExpiresAt > DateTime.Now))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public int GetNextAvailableSeatNumber(int coachId)
    {
        try
        {
            var existingNumbers = _context.Seats
                .Where(s => s.CoachId == coachId)
                .Select(s => s.SeatNumber)
                .OrderBy(n => n)
                .ToList();

            if (!existingNumbers.Any())
                return 1;

            for (int i = 1; i <= existingNumbers.Max() + 1; i++)
            {
                if (!existingNumbers.Contains(i))
                    return i;
            }

            return existingNumbers.Max() + 1;
        }
        catch
        {
            return 1;
        }
    }

    public IEnumerable<Seat> GetSeatsByTypeInCoach(int coachId, int seatTypeId)
    {
        return _context.Seats
            .Include(s => s.Coach)
            .ThenInclude(c => c.Train)
            .Include(s => s.SeatType)
            .Where(s => s.CoachId == coachId && s.SeatTypeId == seatTypeId)
            .OrderBy(s => s.SeatNumber)
            .ToList();
    }
}