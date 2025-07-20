using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class SeatTypeService : ISeatTypeService
{
    private readonly Context _context;

    public SeatTypeService(Context context)
    {
        _context = context;
    }

    public IEnumerable<SeatType> GetAllSeatTypes()
    {
        return _context.SeatTypes
            .Include(st => st.Seats)
            .OrderBy(st => st.TypeName)
            .ToList();
    }

    public SeatType? GetSeatTypeById(int seatTypeId)
    {
        return _context.SeatTypes
            .Include(st => st.Seats)
            .FirstOrDefault(st => st.SeatTypeId == seatTypeId);
    }

    public SeatType? GetSeatTypeByName(string typeName)
    {
        return _context.SeatTypes
            .Include(st => st.Seats)
            .FirstOrDefault(st => st.TypeName.ToLower() == typeName.ToLower());
    }

    public IEnumerable<SeatType> GetSeatTypesByBerthLevel(int? berthLevel)
    {
        return _context.SeatTypes
            .Include(st => st.Seats)
            .Where(st => st.BerthLevel == berthLevel)
            .OrderBy(st => st.TypeName)
            .ToList();
    }

    public IEnumerable<SeatType> GetBerthSeatTypes()
    {
        return _context.SeatTypes
            .Include(st => st.Seats)
            .Where(st => st.BerthLevel.HasValue)
            .OrderBy(st => st.BerthLevel)
            .ThenBy(st => st.TypeName)
            .ToList();
    }

    public IEnumerable<SeatType> GetNonBerthSeatTypes()
    {
        return _context.SeatTypes
            .Include(st => st.Seats)
            .Where(st => !st.BerthLevel.HasValue)
            .OrderBy(st => st.TypeName)
            .ToList();
    }

    public SeatType CreateSeatType(SeatType seatType)
    {
        try
        {
            _context.SeatTypes.Add(seatType);
            _context.SaveChanges();
            return seatType;
        }
        catch
        {
            throw;
        }
    }

    public SeatType? UpdateSeatType(SeatType seatType)
    {
        try
        {
            _context.SeatTypes.Update(seatType);
            _context.SaveChanges();
            return seatType;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteSeatType(int seatTypeId)
    {
        try
        {
            var seatType = _context.SeatTypes.Find(seatTypeId);
            if (seatType == null) return false;

            // Check if seat type is in use
            if (IsSeatTypeInUse(seatTypeId))
                return false;

            _context.SeatTypes.Remove(seatType);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsSeatTypeNameUnique(string typeName, int? excludeSeatTypeId = null)
    {
        try
        {
            var query = _context.SeatTypes
                .Where(st => st.TypeName.ToLower() == typeName.ToLower());

            if (excludeSeatTypeId.HasValue)
            {
                query = query.Where(st => st.SeatTypeId != excludeSeatTypeId.Value);
            }

            return !query.Any();
        }
        catch
        {
            return false;
        }
    }

    public bool IsSeatTypeInUse(int seatTypeId)
    {
        try
        {
            return _context.Seats.Any(s => s.SeatTypeId == seatTypeId);
        }
        catch
        {
            return true; // Assume in use if error occurs for safety
        }
    }
}