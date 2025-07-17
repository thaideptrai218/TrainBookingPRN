using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Coach
{
    public int CoachId { get; set; }

    public int TrainId { get; set; }

    public int CoachNumber { get; set; }

    public string CoachName { get; set; } = null!;

    public int CoachTypeId { get; set; }

    public int Capacity { get; set; }

    public int PositionInTrain { get; set; }

    public bool IsActive { get; set; }

    public virtual CoachType CoachType { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<TemporarySeatHold> TemporarySeatHolds { get; set; } = new List<TemporarySeatHold>();

    public virtual Train Train { get; set; } = null!;
}
