using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class SeatType
{
    public int SeatTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal PriceMultiplier { get; set; }

    public string? Description { get; set; }

    public int? BerthLevel { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
