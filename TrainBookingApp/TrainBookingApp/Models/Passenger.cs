using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Passenger
{
    public int PassengerId { get; set; }

    public string FullName { get; set; } = null!;

    public string? IdcardNumber { get; set; }

    public int PassengerTypeId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int? UserId { get; set; }

    public virtual PassengerType PassengerType { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual User? User { get; set; }
}
