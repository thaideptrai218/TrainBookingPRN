using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Seat
{
    public int SeatId { get; set; }

    public int CoachId { get; set; }

    public int SeatNumber { get; set; }

    public string SeatName { get; set; } = null!;

    public int SeatTypeId { get; set; }

    public bool IsEnabled { get; set; }

    public virtual Coach Coach { get; set; } = null!;

    public virtual SeatType SeatType { get; set; } = null!;

    public virtual ICollection<TemporarySeatHold> TemporarySeatHolds { get; set; } = new List<TemporarySeatHold>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
