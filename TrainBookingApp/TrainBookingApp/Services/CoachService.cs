using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class CoachService : ICoachService
{
    private readonly Context _context;

    public CoachService(Context context)
    {
        _context = context;
    }

    public IEnumerable<Coach> GetAllCoaches()
    {
        return _context.Coaches
            .Include(c => c.Train)
            .Include(c => c.CoachType)
            .Include(c => c.Seats)
            .ThenInclude(s => s.SeatType)
            .OrderBy(c => c.Train.TrainName)
            .ThenBy(c => c.PositionInTrain)
            .ToList();
    }

    public Coach? GetCoachById(int coachId)
    {
        return _context.Coaches
            .Include(c => c.Train)
            .Include(c => c.CoachType)
            .Include(c => c.Seats)
            .ThenInclude(s => s.SeatType)
            .FirstOrDefault(c => c.CoachId == coachId);
    }

    public IEnumerable<Coach> GetActiveCoaches()
    {
        return _context.Coaches
            .Include(c => c.Train)
            .Include(c => c.CoachType)
            .Include(c => c.Seats)
            .ThenInclude(s => s.SeatType)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Train.TrainName)
            .ThenBy(c => c.PositionInTrain)
            .ToList();
    }

    public IEnumerable<Coach> GetCoachesByTrainId(int trainId)
    {
        return _context.Coaches
            .Include(c => c.Train)
            .Include(c => c.CoachType)
            .Include(c => c.Seats)
            .ThenInclude(s => s.SeatType)
            .Where(c => c.TrainId == trainId)
            .OrderBy(c => c.PositionInTrain)
            .ToList();
    }

    public IEnumerable<Coach> GetActiveCoachesByTrainId(int trainId)
    {
        return _context.Coaches
            .Include(c => c.Train)
            .Include(c => c.CoachType)
            .Include(c => c.Seats)
            .ThenInclude(s => s.SeatType)
            .Where(c => c.TrainId == trainId && c.IsActive)
            .OrderBy(c => c.PositionInTrain)
            .ToList();
    }

    public Coach CreateCoach(Coach coach)
    {
        try
        {
            _context.Coaches.Add(coach);
            _context.SaveChanges();
            return coach;
        }
        catch
        {
            throw;
        }
    }

    public Coach? UpdateCoach(Coach coach)
    {
        try
        {
            _context.Coaches.Update(coach);
            _context.SaveChanges();
            return coach;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteCoach(int coachId)
    {
        try
        {
            var coach = _context.Coaches.Find(coachId);
            if (coach == null) return false;

            _context.Coaches.Remove(coach);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ActivateCoach(int coachId)
    {
        try
        {
            var coach = _context.Coaches.Find(coachId);
            if (coach == null) return false;

            coach.IsActive = true;
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DeactivateCoach(int coachId)
    {
        try
        {
            var coach = _context.Coaches.Find(coachId);
            if (coach == null) return false;

            coach.IsActive = false;
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsCoachNumberUniqueInTrain(int trainId, int coachNumber, int? excludeCoachId = null)
    {
        try
        {
            var query = _context.Coaches
                .Where(c => c.TrainId == trainId && c.CoachNumber == coachNumber);

            if (excludeCoachId.HasValue)
            {
                query = query.Where(c => c.CoachId != excludeCoachId.Value);
            }

            return !query.Any();
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateCoachPosition(int trainId, int positionInTrain, int? excludeCoachId = null)
    {
        try
        {
            var query = _context.Coaches
                .Where(c => c.TrainId == trainId && c.PositionInTrain == positionInTrain);

            if (excludeCoachId.HasValue)
            {
                query = query.Where(c => c.CoachId != excludeCoachId.Value);
            }

            return !query.Any();
        }
        catch
        {
            return false;
        }
    }

    public int GetNextAvailableCoachNumber(int trainId)
    {
        try
        {
            var existingNumbers = _context.Coaches
                .Where(c => c.TrainId == trainId)
                .Select(c => c.CoachNumber)
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

    public int GetNextAvailablePosition(int trainId)
    {
        try
        {
            var existingPositions = _context.Coaches
                .Where(c => c.TrainId == trainId)
                .Select(c => c.PositionInTrain)
                .OrderBy(p => p)
                .ToList();

            if (!existingPositions.Any())
                return 1;

            for (int i = 1; i <= existingPositions.Max() + 1; i++)
            {
                if (!existingPositions.Contains(i))
                    return i;
            }

            return existingPositions.Max() + 1;
        }
        catch
        {
            return 1;
        }
    }
}