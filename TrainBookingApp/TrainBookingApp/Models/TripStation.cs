using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class TripStation
{
    public int TripStationId { get; set; }

    public int TripId { get; set; }

    public int StationId { get; set; }

    public int SequenceNumber { get; set; }

    public DateTime? ScheduledArrival { get; set; }

    public DateTime? ScheduledDeparture { get; set; }

    public DateTime? ActualArrival { get; set; }

    public DateTime? ActualDeparture { get; set; }

    public virtual Station Station { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
}
