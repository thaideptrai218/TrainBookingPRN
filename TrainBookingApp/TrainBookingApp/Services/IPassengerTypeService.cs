using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface IPassengerTypeService
{
    List<PassengerType> GetAllPassengerTypes();
    PassengerType? GetPassengerTypeById(int id);
    bool CreatePassengerType(PassengerType passengerType);
    bool UpdatePassengerType(PassengerType passengerType);
    bool DeletePassengerType(int id);
    bool IsPassengerTypeNameUnique(string typeName, int? excludeId = null);
}