using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface IBookingService
{
    IEnumerable<Booking> GetUserBookings(int userId);
    IEnumerable<Seat> GetAvailableSeats(int tripId, int? coachId = null);
    bool IsValidBookingData(Booking booking);
    bool CreateBooking(Booking booking, List<Passenger> passengers, int tripId, List<int> seatIds);
    bool CancelBooking(int bookingId, string reason);
    decimal CalculateTotalPrice(int tripId, List<int> seatIds, bool isRoundTrip = false);
    bool HoldSeats(int tripId, List<int> seatIds, int userId, int holdDurationMinutes = 15);
    bool ReleaseSeats(int tripId, List<int> seatIds, int userId);
    IEnumerable<Coach> GetTripCoaches(int tripId);
    Booking? GetBookingById(int bookingId);
    bool ProcessPayment(int bookingId, decimal amount, string paymentMethod);
}