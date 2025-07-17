using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface IRouteService
{
    IEnumerable<Route> GetAllRoutes();
    Route? GetRouteById(int routeId);
    IEnumerable<Route> GetRoutesByStation(int stationId);
    Route CreateRoute(Route route);
    Route? UpdateRoute(Route route);
    bool DeleteRoute(int routeId);
    IEnumerable<Station> GetAllStations();
    
    // Route Station Management
    bool AddStationToRoute(int routeId, int stationId, int sequenceNumber, decimal distanceFromStart, int defaultStopTime);
    bool RemoveStationFromRoute(int routeId, int stationId);
    bool UpdateRouteStationSequence(int routeId, int stationId, int newSequenceNumber);
    IEnumerable<RouteStation> GetRouteStations(int routeId);
    bool IsRouteNameExists(string routeName);
}