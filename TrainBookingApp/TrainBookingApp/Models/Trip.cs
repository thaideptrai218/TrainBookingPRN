using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Trip
{
    public int TripId { get; set; }

    public int TrainId { get; set; }

    public int RouteId { get; set; }

    public DateTime DepartureDateTime { get; set; }

    public DateTime ArrivalDateTime { get; set; }

    public bool IsHolidayTrip { get; set; }

    public string TripStatus { get; set; } = null!;

    public decimal BasePriceMultiplier { get; set; }

    public virtual Route Route { get; set; } = null!;

    public virtual ICollection<TemporarySeatHold> TemporarySeatHolds { get; set; } = new List<TemporarySeatHold>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual Train Train { get; set; } = null!;

    public virtual ICollection<TripStation> TripStations { get; set; } = new List<TripStation>();
}
