using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface ISeatTypeService
{
    IEnumerable<SeatType> GetAllSeatTypes();
    SeatType? GetSeatTypeById(int seatTypeId);
    SeatType? GetSeatTypeByName(string typeName);
    IEnumerable<SeatType> GetSeatTypesByBerthLevel(int? berthLevel);
    IEnumerable<SeatType> GetBerthSeatTypes();
    IEnumerable<SeatType> GetNonBerthSeatTypes();
    SeatType CreateSeatType(SeatType seatType);
    SeatType? UpdateSeatType(SeatType seatType);
    bool DeleteSeatType(int seatTypeId);
    bool IsSeatTypeNameUnique(string typeName, int? excludeSeatTypeId = null);
    bool IsSeatTypeInUse(int seatTypeId);
}