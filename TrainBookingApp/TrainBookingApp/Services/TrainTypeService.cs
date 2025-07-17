using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class TrainTypeService : ITrainTypeService
{
    private readonly Context _context;

    public TrainTypeService(Context context)
    {
        _context = context;
    }

    public IEnumerable<TrainType> GetAllTrainTypes()
    {
        return _context.TrainTypes
            .OrderBy(tt => tt.TypeName)
            .ToList();
    }

    public TrainType? GetTrainTypeById(int trainTypeId)
    {
        return _context.TrainTypes
            .FirstOrDefault(tt => tt.TrainTypeId == trainTypeId);
    }

    public TrainType CreateTrainType(TrainType trainType)
    {
        _context.TrainTypes.Add(trainType);
        _context.SaveChanges();
        return trainType;
    }

    public TrainType? UpdateTrainType(TrainType trainType)
    {
        try
        {
            _context.TrainTypes.Update(trainType);
            _context.SaveChanges();
            return trainType;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteTrainType(int trainTypeId)
    {
        try
        {
            var trainType = _context.TrainTypes.Find(trainTypeId);
            if (trainType == null) return false;

            // Check if train type is used by any trains
            var hasTrains = _context.Trains.Any(t => t.TrainTypeId == trainTypeId);
            if (hasTrains)
            {
                return false; // Cannot delete train type that is used by trains
            }

            _context.TrainTypes.Remove(trainType);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsTrainTypeNameExists(string typeName)
    {
        return _context.TrainTypes.Any(tt => tt.TypeName == typeName);
    }

    public IEnumerable<TrainType> SearchTrainTypes(string searchTerm)
    {
        return _context.TrainTypes
            .Where(tt => tt.TypeName.Contains(searchTerm) || 
                        (tt.Description != null && tt.Description.Contains(searchTerm)))
            .OrderBy(tt => tt.TypeName)
            .ToList();
    }
}