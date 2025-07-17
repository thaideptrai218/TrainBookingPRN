using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface IStationService
{
    IEnumerable<Station> GetAllStations();
    Station? GetStationById(int stationId);
    Station? GetStationByCode(string stationCode);
    IEnumerable<Station> GetStationsByCity(string city);
    IEnumerable<Station> GetStationsByRegion(string region);
    Station CreateStation(Station station);
    Station? UpdateStation(Station station);
    bool DeleteStation(int stationId);
    bool IsStationCodeExists(string stationCode);
    IEnumerable<Station> SearchStations(string searchTerm);
}