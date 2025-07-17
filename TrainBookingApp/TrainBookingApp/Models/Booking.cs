using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public string BookingCode { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime BookingDateTime { get; set; }

    public decimal TotalPrice { get; set; }

    public string BookingStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? Source { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual User User { get; set; } = null!;
}
