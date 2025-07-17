using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class TrainType
{
    public int TrainTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal? AverageVelocity { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();

    public virtual ICollection<Train> Trains { get; set; } = new List<Train>();
}
