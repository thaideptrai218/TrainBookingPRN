using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class PassengerType
{
    public int PassengerTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal DiscountPercentage { get; set; }

    public string? Description { get; set; }

    public bool RequiresDocument { get; set; }

    public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
}
