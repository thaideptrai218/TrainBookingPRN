using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public string TicketCode { get; set; } = null!;

    public int BookingId { get; set; }

    public int TripId { get; set; }

    public int SeatId { get; set; }

    public int PassengerId { get; set; }

    public int StartStationId { get; set; }

    public int EndStationId { get; set; }

    public decimal Price { get; set; }

    public string TicketStatus { get; set; } = null!;

    public string CoachNameSnapshot { get; set; } = null!;

    public string SeatNameSnapshot { get; set; } = null!;

    public string PassengerNameSnapshot { get; set; } = null!;

    public string? PassengerIdcardNumberSnapshot { get; set; }

    public string? FareComponentDetails { get; set; }

    public int? ParentTicketId { get; set; }

    public bool? IsRefundable { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Station EndStation { get; set; } = null!;

    public virtual ICollection<Ticket> InverseParentTicket { get; set; } = new List<Ticket>();

    public virtual Ticket? ParentTicket { get; set; }

    public virtual Passenger Passenger { get; set; } = null!;

    public virtual Refund? Refund { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual Station StartStation { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
}
