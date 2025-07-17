using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface ITrainService
{
    IEnumerable<Train> GetAllTrains();
    Train? GetTrainById(int trainId);
    IEnumerable<Train> GetActiveTrains();
    Train CreateTrain(Train train);
    Train? UpdateTrain(Train train);
    bool DeleteTrain(int trainId);
    bool ActivateTrain(int trainId);
    bool DeactivateTrain(int trainId);
}