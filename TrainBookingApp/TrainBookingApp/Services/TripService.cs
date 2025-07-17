using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class TripService : ITripService
{
    private readonly Context _context;

    public TripService(Context context)
    {
        _context = context;
    }

    public IEnumerable<Trip> GetAllTrips()
    {
        return _context.Trips
            .Include(t => t.Train)
            .Include(t => t.Route)
            .Include(t => t.TripStations)
            .ThenInclude(ts => ts.Station)
            .OrderBy(t => t.DepartureDateTime)
            .ToList();
    }

    public Trip? GetTripById(int tripId)
    {
        return _context.Trips
            .Include(t => t.Train)
            .Include(t => t.Route)
            .Include(t => t.TripStations)
            .ThenInclude(ts => ts.Station)
            .FirstOrDefault(t => t.TripId == tripId);
    }

    public IEnumerable<Trip> GetTripsByRoute(int routeId)
    {
        return _context.Trips
            .Include(t => t.Train)
            .Include(t => t.Route)
            .Where(t => t.RouteId == routeId)
            .OrderBy(t => t.DepartureDateTime)
            .ToList();
    }

    public IEnumerable<Trip> GetTripsByDateRange(DateTime startDate, DateTime endDate)
    {
        return _context.Trips
            .Include(t => t.Train)
            .Include(t => t.Route)
            .Where(t => t.DepartureDateTime >= startDate && t.DepartureDateTime <= endDate)
            .OrderBy(t => t.DepartureDateTime)
            .ToList();
    }

    public Trip CreateTrip(Trip trip)
    {
        _context.Trips.Add(trip);
        _context.SaveChanges();
        return trip;
    }

    public Trip? UpdateTrip(Trip trip)
    {
        try
        {
            _context.Trips.Update(trip);
            _context.SaveChanges();
            return trip;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteTrip(int tripId)
    {
        try
        {
            var trip = _context.Trips.Find(tripId);
            if (trip == null) return false;

            _context.Trips.Remove(trip);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool CancelTrip(int tripId)
    {
        try
        {
            var trip = _context.Trips.Find(tripId);
            if (trip == null) return false;

            trip.TripStatus = "Cancelled";
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<Trip> SearchTrips(int fromStationId, int toStationId, DateTime departureDate)
    {
        var trips = _context.Trips
            .Include(t => t.Train)
            .Include(t => t.Route)
            .Include(t => t.TripStations)
            .ThenInclude(ts => ts.Station)
            .Where(t => t.DepartureDateTime.Date == departureDate.Date && t.TripStatus == "Active")
            .ToList();

        return trips.Where(t => 
            t.TripStations.Any(ts => ts.StationId == fromStationId) &&
            t.TripStations.Any(ts => ts.StationId == toStationId) &&
            t.TripStations.First(ts => ts.StationId == fromStationId).ScheduledArrival <
            t.TripStations.First(ts => ts.StationId == toStationId).ScheduledArrival
        );
    }
}