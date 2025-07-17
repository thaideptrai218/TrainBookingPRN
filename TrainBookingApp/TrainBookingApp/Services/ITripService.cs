using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface ITripService
{
    IEnumerable<Trip> GetAllTrips();
    Trip? GetTripById(int tripId);
    IEnumerable<Trip> GetTripsByRoute(int routeId);
    IEnumerable<Trip> GetTripsByDateRange(DateTime startDate, DateTime endDate);
    Trip CreateTrip(Trip trip);
    Trip? UpdateTrip(Trip trip);
    bool DeleteTrip(int tripId);
    bool CancelTrip(int tripId);
    IEnumerable<Trip> SearchTrips(int fromStationId, int toStationId, DateTime departureDate);
}