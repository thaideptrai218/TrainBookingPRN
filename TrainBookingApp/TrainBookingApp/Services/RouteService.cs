using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class RouteService : IRouteService
{
    private readonly Context _context;

    public RouteService(Context context)
    {
        _context = context;
    }

    public IEnumerable<Route> GetAllRoutes()
    {
        return _context.Routes
            .Include(r => r.RouteStations)
            .ThenInclude(rs => rs.Station)
            .OrderBy(r => r.RouteName)
            .ToList();
    }

    public Route? GetRouteById(int routeId)
    {
        return _context.Routes
            .Include(r => r.RouteStations)
            .ThenInclude(rs => rs.Station)
            .FirstOrDefault(r => r.RouteId == routeId);
    }

    public IEnumerable<Route> GetRoutesByStation(int stationId)
    {
        return _context.Routes
            .Include(r => r.RouteStations)
            .ThenInclude(rs => rs.Station)
            .Where(r => r.RouteStations.Any(rs => rs.StationId == stationId))
            .OrderBy(r => r.RouteName)
            .ToList();
    }

    public Route CreateRoute(Route route)
    {
        _context.Routes.Add(route);
        _context.SaveChanges();
        return route;
    }

    public Route? UpdateRoute(Route route)
    {
        try
        {
            _context.Routes.Update(route);
            _context.SaveChanges();
            return route;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteRoute(int routeId)
    {
        try
        {
            var route = _context.Routes.Find(routeId);
            if (route == null) return false;

            _context.Routes.Remove(route);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<Station> GetAllStations()
    {
        return _context.Stations
            .OrderBy(s => s.StationName)
            .ToList();
    }

    public bool AddStationToRoute(int routeId, int stationId, int sequenceNumber, decimal distanceFromStart, int defaultStopTime)
    {
        try
        {
            // Check if route exists
            var route = _context.Routes.Find(routeId);
            if (route == null) return false;

            // Check if station exists
            var station = _context.Stations.Find(stationId);
            if (station == null) return false;

            // Check if station is already in route
            var existingRouteStation = _context.RouteStations
                .FirstOrDefault(rs => rs.RouteId == routeId && rs.StationId == stationId);
            if (existingRouteStation != null) return false;

            // Create new route station
            var routeStation = new RouteStation
            {
                RouteId = routeId,
                StationId = stationId,
                SequenceNumber = sequenceNumber,
                DistanceFromStart = distanceFromStart,
                DefaultStopTime = defaultStopTime
            };

            _context.RouteStations.Add(routeStation);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool RemoveStationFromRoute(int routeId, int stationId)
    {
        try
        {
            var routeStation = _context.RouteStations
                .FirstOrDefault(rs => rs.RouteId == routeId && rs.StationId == stationId);
            
            if (routeStation == null) return false;

            _context.RouteStations.Remove(routeStation);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool UpdateRouteStationSequence(int routeId, int stationId, int newSequenceNumber)
    {
        try
        {
            var routeStation = _context.RouteStations
                .FirstOrDefault(rs => rs.RouteId == routeId && rs.StationId == stationId);
            
            if (routeStation == null) return false;

            routeStation.SequenceNumber = newSequenceNumber;
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<RouteStation> GetRouteStations(int routeId)
    {
        return _context.RouteStations
            .Include(rs => rs.Station)
            .Where(rs => rs.RouteId == routeId)
            .OrderBy(rs => rs.SequenceNumber)
            .ToList();
    }

    public bool IsRouteNameExists(string routeName)
    {
        return _context.Routes.Any(r => r.RouteName == routeName);
    }
}