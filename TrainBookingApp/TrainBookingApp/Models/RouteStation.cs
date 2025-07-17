using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class RouteStation
{
    public int RouteStationId { get; set; }

    public int RouteId { get; set; }

    public int StationId { get; set; }

    public int SequenceNumber { get; set; }

    public decimal DistanceFromStart { get; set; }

    public int DefaultStopTime { get; set; }

    public virtual Route Route { get; set; } = null!;

    public virtual Station Station { get; set; } = null!;
}
