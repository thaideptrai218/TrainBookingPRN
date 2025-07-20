using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class CoachTypeService : ICoachTypeService
{
    private readonly Context _context;

    public CoachTypeService(Context context)
    {
        _context = context;
    }

    public IEnumerable<CoachType> GetAllCoachTypes()
    {
        return _context.CoachTypes
            .Include(ct => ct.Coaches)
            .OrderBy(ct => ct.TypeName)
            .ToList();
    }

    public CoachType? GetCoachTypeById(int coachTypeId)
    {
        return _context.CoachTypes
            .Include(ct => ct.Coaches)
            .FirstOrDefault(ct => ct.CoachTypeId == coachTypeId);
    }

    public CoachType? GetCoachTypeByName(string typeName)
    {
        return _context.CoachTypes
            .Include(ct => ct.Coaches)
            .FirstOrDefault(ct => ct.TypeName.ToLower() == typeName.ToLower());
    }

    public IEnumerable<CoachType> GetCompartmentedCoachTypes()
    {
        return _context.CoachTypes
            .Include(ct => ct.Coaches)
            .Where(ct => ct.IsCompartmented)
            .OrderBy(ct => ct.TypeName)
            .ToList();
    }

    public IEnumerable<CoachType> GetNonCompartmentedCoachTypes()
    {
        return _context.CoachTypes
            .Include(ct => ct.Coaches)
            .Where(ct => !ct.IsCompartmented)
            .OrderBy(ct => ct.TypeName)
            .ToList();
    }

    public CoachType CreateCoachType(CoachType coachType)
    {
        try
        {
            _context.CoachTypes.Add(coachType);
            _context.SaveChanges();
            return coachType;
        }
        catch
        {
            throw;
        }
    }

    public CoachType? UpdateCoachType(CoachType coachType)
    {
        try
        {
            _context.CoachTypes.Update(coachType);
            _context.SaveChanges();
            return coachType;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteCoachType(int coachTypeId)
    {
        try
        {
            var coachType = _context.CoachTypes.Find(coachTypeId);
            if (coachType == null) return false;

            // Check if coach type is in use
            if (IsCoachTypeInUse(coachTypeId))
                return false;

            _context.CoachTypes.Remove(coachType);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsCoachTypeNameUnique(string typeName, int? excludeCoachTypeId = null)
    {
        try
        {
            var query = _context.CoachTypes
                .Where(ct => ct.TypeName.ToLower() == typeName.ToLower());

            if (excludeCoachTypeId.HasValue)
            {
                query = query.Where(ct => ct.CoachTypeId != excludeCoachTypeId.Value);
            }

            return !query.Any();
        }
        catch
        {
            return false;
        }
    }

    public bool IsCoachTypeInUse(int coachTypeId)
    {
        try
        {
            return _context.Coaches.Any(c => c.CoachTypeId == coachTypeId);
        }
        catch
        {
            return true; // Assume in use if error occurs for safety
        }
    }
}