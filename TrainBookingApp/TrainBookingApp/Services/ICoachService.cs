using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface ICoachService
{
    IEnumerable<Coach> GetAllCoaches();
    Coach? GetCoachById(int coachId);
    IEnumerable<Coach> GetActiveCoaches();
    IEnumerable<Coach> GetCoachesByTrainId(int trainId);
    IEnumerable<Coach> GetActiveCoachesByTrainId(int trainId);
    Coach CreateCoach(Coach coach);
    Coach? UpdateCoach(Coach coach);
    bool DeleteCoach(int coachId);
    bool ActivateCoach(int coachId);
    bool DeactivateCoach(int coachId);
    bool IsCoachNumberUniqueInTrain(int trainId, int coachNumber, int? excludeCoachId = null);
    bool ValidateCoachPosition(int trainId, int positionInTrain, int? excludeCoachId = null);
    int GetNextAvailableCoachNumber(int trainId);
    int GetNextAvailablePosition(int trainId);
}