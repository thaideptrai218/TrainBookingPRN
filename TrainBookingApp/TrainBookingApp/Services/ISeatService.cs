using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface ISeatService
{
    IEnumerable<Seat> GetAllSeats();
    Seat? GetSeatById(int seatId);
    IEnumerable<Seat> GetEnabledSeats();
    IEnumerable<Seat> GetSeatsByCoachId(int coachId);
    IEnumerable<Seat> GetEnabledSeatsByCoachId(int coachId);
    IEnumerable<Seat> GetSeatsByTrainId(int trainId);
    IEnumerable<Seat> GetAvailableSeatsForTrip(int tripId, int coachId);
    Seat CreateSeat(Seat seat);
    Seat? UpdateSeat(Seat seat);
    bool DeleteSeat(int seatId);
    bool EnableSeat(int seatId);
    bool DisableSeat(int seatId);
    bool IsSeatNumberUniqueInCoach(int coachId, int seatNumber, int? excludeSeatId = null);
    bool IsSeatAvailableForTrip(int seatId, int tripId);
    int GetNextAvailableSeatNumber(int coachId);
    IEnumerable<Seat> GetSeatsByTypeInCoach(int coachId, int seatTypeId);
}