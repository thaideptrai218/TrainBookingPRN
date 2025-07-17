using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class TemporarySeatHold
{
    public int HoldId { get; set; }

    public int TripId { get; set; }

    public int SeatId { get; set; }

    public int CoachId { get; set; }

    public int LegOriginStationId { get; set; }

    public int LegDestinationStationId { get; set; }

    public string SessionId { get; set; } = null!;

    public int? UserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Coach Coach { get; set; } = null!;

    public virtual Station LegDestinationStation { get; set; } = null!;

    public virtual Station LegOriginStation { get; set; } = null!;

    public virtual Seat Seat { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;

    public virtual User? User { get; set; }
}
