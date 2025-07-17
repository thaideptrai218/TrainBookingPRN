using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Station
{
    public int StationId { get; set; }

    public string StationCode { get; set; } = null!;

    public string StationName { get; set; } = null!;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual ICollection<RouteStation> RouteStations { get; set; } = new List<RouteStation>();

    public virtual ICollection<TemporarySeatHold> TemporarySeatHoldLegDestinationStations { get; set; } = new List<TemporarySeatHold>();

    public virtual ICollection<TemporarySeatHold> TemporarySeatHoldLegOriginStations { get; set; } = new List<TemporarySeatHold>();

    public virtual ICollection<Ticket> TicketEndStations { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketStartStations { get; set; } = new List<Ticket>();

    public virtual ICollection<TripStation> TripStations { get; set; } = new List<TripStation>();
}
