using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class TripService : ITripService
{
    private readonly Context _context;
    private readonly IRouteService _routeService;

    public TripService(Context context, IRouteService routeService)
    {
        _context = context;
        _routeService = routeService;
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
        try
        {
            // Add the trip first
            _context.Trips.Add(trip);
            _context.SaveChanges();

            // Automatically create TripStations based on the route
            CreateTripStationsFromRoute(trip);

            return trip;
        }
        catch
        {
            throw;
        }
    }

    private void CreateTripStationsFromRoute(Trip trip)
    {
        // Get all route stations for this trip's route, ordered by sequence
        var routeStations = _routeService.GetRouteStations(trip.RouteId).ToList();

        if (!routeStations.Any()) return;

        var tripStations = new List<TripStation>();
        var currentDateTime = trip.DepartureDateTime;

        for (int i = 0; i < routeStations.Count; i++)
        {
            var routeStation = routeStations[i];
            var tripStation = new TripStation
            {
                TripId = trip.TripId,
                StationId = routeStation.StationId,
                SequenceNumber = routeStation.SequenceNumber
            };

            if (i == 0)
            {
                // First station: arrives and departs at trip departure time
                tripStation.ScheduledArrival = currentDateTime;
                tripStation.ScheduledDeparture = currentDateTime.AddMinutes(routeStation.DefaultStopTime);
                currentDateTime = tripStation.ScheduledDeparture.Value;
            }
            else if (i == routeStations.Count - 1)
            {
                // Last station: arrives at trip arrival time, no departure
                tripStation.ScheduledArrival = trip.ArrivalDateTime;
                tripStation.ScheduledDeparture = null;
            }
            else
            {
                // Intermediate stations: calculate arrival based on distance and add stop time
                var prevStation = routeStations[i - 1];
                var distanceDiff = routeStation.DistanceFromStart - prevStation.DistanceFromStart;

                // Assume average speed of 60 km/h to calculate travel time
                var travelTimeMinutes = (double)(distanceDiff * 60 / 60); // distance in km, speed 60 km/h

                tripStation.ScheduledArrival = currentDateTime.AddMinutes(travelTimeMinutes);
                tripStation.ScheduledDeparture = tripStation.ScheduledArrival.Value.AddMinutes(routeStation.DefaultStopTime);
                currentDateTime = tripStation.ScheduledDeparture.Value;
            }

            tripStations.Add(tripStation);
        }

        // Save all trip stations
        _context.TripStations.AddRange(tripStations);
        _context.SaveChanges();
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
            .Where(t => t.DepartureDateTime.Date == departureDate.Date && t.TripStatus == "Scheduled")
            .ToList();

        return trips.Where(t =>
            t.TripStations.Any(ts => ts.StationId == fromStationId) &&
            t.TripStations.Any(ts => ts.StationId == toStationId) &&
            t.TripStations.First(ts => ts.StationId == fromStationId).ScheduledArrival <
            t.TripStations.First(ts => ts.StationId == toStationId).ScheduledArrival
        );
    }
}