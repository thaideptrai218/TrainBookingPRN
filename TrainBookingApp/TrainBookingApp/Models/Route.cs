using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Route
{
    public int RouteId { get; set; }

    public string RouteName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();

    public virtual ICollection<RouteStation> RouteStations { get; set; } = new List<RouteStation>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
