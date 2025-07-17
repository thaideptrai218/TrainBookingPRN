using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class PricingRule
{
    public int RuleId { get; set; }

    public string RuleName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal BasePricePerKm { get; set; }

    public int? TrainTypeId { get; set; }

    public int? RouteId { get; set; }

    public bool? IsForRoundTrip { get; set; }

    public DateOnly? ApplicableDateStart { get; set; }

    public DateOnly? ApplicableDateEnd { get; set; }

    public int Priority { get; set; }

    public bool IsActive { get; set; }

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public virtual Route? Route { get; set; }

    public virtual TrainType? TrainType { get; set; }
}
