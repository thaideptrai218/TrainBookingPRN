using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface ITrainTypeService
{
    IEnumerable<TrainType> GetAllTrainTypes();
    TrainType? GetTrainTypeById(int trainTypeId);
    TrainType CreateTrainType(TrainType trainType);
    TrainType? UpdateTrainType(TrainType trainType);
    bool DeleteTrainType(int trainTypeId);
    bool IsTrainTypeNameExists(string typeName);
    IEnumerable<TrainType> SearchTrainTypes(string searchTerm);
}