using TrainBookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace TrainBookingApp.Services;

public class PassengerTypeService : IPassengerTypeService
{
    private readonly Context _context;

    public PassengerTypeService(Context context)
    {
        _context = context;
    }

    public List<PassengerType> GetAllPassengerTypes()
    {
        try
        {
            return _context.PassengerTypes
                .OrderBy(pt => pt.TypeName)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving passenger types: {ex.Message}", ex);
        }
    }

    public PassengerType? GetPassengerTypeById(int id)
    {
        try
        {
            return _context.PassengerTypes
                .FirstOrDefault(pt => pt.PassengerTypeId == id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving passenger type with ID {id}: {ex.Message}", ex);
        }
    }

    public bool CreatePassengerType(PassengerType passengerType)
    {
        try
        {
            _context.PassengerTypes.Add(passengerType);
            return _context.SaveChanges() > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating passenger type: {ex.Message}", ex);
        }
    }

    public bool UpdatePassengerType(PassengerType passengerType)
    {
        try
        {
            _context.PassengerTypes.Update(passengerType);
            return _context.SaveChanges() > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating passenger type: {ex.Message}", ex);
        }
    }

    public bool DeletePassengerType(int id)
    {
        try
        {
            var passengerType = GetPassengerTypeById(id);
            if (passengerType == null)
                return false;

            _context.PassengerTypes.Remove(passengerType);
            return _context.SaveChanges() > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting passenger type: {ex.Message}", ex);
        }
    }

    public bool IsPassengerTypeNameUnique(string typeName, int? excludeId = null)
    {
        try
        {
            var query = _context.PassengerTypes.Where(pt => pt.TypeName == typeName);
            
            if (excludeId.HasValue)
            {
                query = query.Where(pt => pt.PassengerTypeId != excludeId.Value);
            }
            
            return !query.Any();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error checking passenger type name uniqueness: {ex.Message}", ex);
        }
    }
}