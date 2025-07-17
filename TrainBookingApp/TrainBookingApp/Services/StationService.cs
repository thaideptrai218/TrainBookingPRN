using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class StationService : IStationService
{
    private readonly Context _context;

    public StationService(Context context)
    {
        _context = context;
    }

    public IEnumerable<Station> GetAllStations()
    {
        return _context.Stations
            .OrderBy(s => s.StationName)
            .ToList();
    }

    public Station? GetStationById(int stationId)
    {
        return _context.Stations
            .FirstOrDefault(s => s.StationId == stationId);
    }

    public Station? GetStationByCode(string stationCode)
    {
        return _context.Stations
            .FirstOrDefault(s => s.StationCode == stationCode);
    }

    public IEnumerable<Station> GetStationsByCity(string city)
    {
        return _context.Stations
            .Where(s => s.City == city)
            .OrderBy(s => s.StationName)
            .ToList();
    }

    public IEnumerable<Station> GetStationsByRegion(string region)
    {
        return _context.Stations
            .Where(s => s.Region == region)
            .OrderBy(s => s.StationName)
            .ToList();
    }

    public Station CreateStation(Station station)
    {
        _context.Stations.Add(station);
        _context.SaveChanges();
        return station;
    }

    public Station? UpdateStation(Station station)
    {
        try
        {
            _context.Stations.Update(station);
            _context.SaveChanges();
            return station;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteStation(int stationId)
    {
        try
        {
            var station = _context.Stations.Find(stationId);
            if (station == null) return false;

            // Check if station is used in any routes
            var hasRoutes = _context.RouteStations.Any(rs => rs.StationId == stationId);
            if (hasRoutes)
            {
                return false; // Cannot delete station that is part of routes
            }

            _context.Stations.Remove(station);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsStationCodeExists(string stationCode)
    {
        return _context.Stations.Any(s => s.StationCode == stationCode);
    }

    public IEnumerable<Station> SearchStations(string searchTerm)
    {
        return _context.Stations
            .Where(s => s.StationName.Contains(searchTerm) || 
                       s.StationCode.Contains(searchTerm) ||
                       (s.City != null && s.City.Contains(searchTerm)) ||
                       (s.Region != null && s.Region.Contains(searchTerm)))
            .OrderBy(s => s.StationName)
            .ToList();
    }
}