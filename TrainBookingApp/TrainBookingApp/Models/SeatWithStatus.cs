using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class SeatWithStatus
{
    public Seat Seat { get; set; } = null!;
    public SeatStatus Status { get; set; }
    public bool IsSelected { get; set; }
}

public enum SeatStatus
{
    Available,
    Occupied, // Booked by someone else
    Held,     // Temporarily held by someone else
    Selected  // Selected by current user
}