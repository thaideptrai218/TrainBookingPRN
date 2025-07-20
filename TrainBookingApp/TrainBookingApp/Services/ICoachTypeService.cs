using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface ICoachTypeService
{
    IEnumerable<CoachType> GetAllCoachTypes();
    CoachType? GetCoachTypeById(int coachTypeId);
    CoachType? GetCoachTypeByName(string typeName);
    IEnumerable<CoachType> GetCompartmentedCoachTypes();
    IEnumerable<CoachType> GetNonCompartmentedCoachTypes();
    CoachType CreateCoachType(CoachType coachType);
    CoachType? UpdateCoachType(CoachType coachType);
    bool DeleteCoachType(int coachTypeId);
    bool IsCoachTypeNameUnique(string typeName, int? excludeCoachTypeId = null);
    bool IsCoachTypeInUse(int coachTypeId);
}