using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class CancellationPolicy
{
    public int PolicyId { get; set; }

    public string PolicyName { get; set; } = null!;

    public int HoursBeforeDepartureMin { get; set; }

    public int? HoursBeforeDepartureMax { get; set; }

    public decimal? FeePercentage { get; set; }

    public decimal? FixedFeeAmount { get; set; }

    public bool IsRefundable { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
