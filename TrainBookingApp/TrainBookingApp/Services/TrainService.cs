using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class TrainService : ITrainService
{
    private readonly Context _context;

    public TrainService(Context context)
    {
        _context = context;
    }

    public IEnumerable<Train> GetAllTrains()
    {
        return _context.Trains
            .Include(t => t.TrainType)
            .Include(t => t.Coaches)
            .ThenInclude(c => c.CoachType)
            .OrderBy(t => t.TrainName)
            .ToList();
    }

    public Train? GetTrainById(int trainId)
    {
        return _context.Trains
            .Include(t => t.TrainType)
            .Include(t => t.Coaches)
            .ThenInclude(c => c.CoachType)
            .FirstOrDefault(t => t.TrainId == trainId);
    }

    public IEnumerable<Train> GetActiveTrains()
    {
        return _context.Trains
            .Include(t => t.TrainType)
            .Include(t => t.Coaches)
            .ThenInclude(c => c.CoachType)
            .Where(t => t.IsActive)
            .OrderBy(t => t.TrainName)
            .ToList();
    }

    public Train CreateTrain(Train train)
    {
        _context.Trains.Add(train);
        _context.SaveChanges();
        return train;
    }

    public Train? UpdateTrain(Train train)
    {
        try
        {
            _context.Trains.Update(train);
            _context.SaveChanges();
            return train;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteTrain(int trainId)
    {
        try
        {
            var train = _context.Trains.Find(trainId);
            if (train == null) return false;

            _context.Trains.Remove(train);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ActivateTrain(int trainId)
    {
        try
        {
            var train = _context.Trains.Find(trainId);
            if (train == null) return false;

            train.IsActive = true;
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DeactivateTrain(int trainId)
    {
        try
        {
            var train = _context.Trains.Find(trainId);
            if (train == null) return false;

            train.IsActive = false;
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }
}