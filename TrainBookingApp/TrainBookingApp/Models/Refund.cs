using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Refund
{
    public int RefundId { get; set; }

    public int TicketId { get; set; }

    public int BookingId { get; set; }

    public int? AppliedPolicyId { get; set; }

    public decimal OriginalTicketPrice { get; set; }

    public decimal FeeAmount { get; set; }

    public decimal ActualRefundAmount { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? RefundMethod { get; set; }

    public string? Notes { get; set; }

    public int? RequestedByUserId { get; set; }

    public int? ProcessedByUserId { get; set; }

    public string? RefundTransactionId { get; set; }

    public virtual CancellationPolicy? AppliedPolicy { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User? ProcessedByUser { get; set; }

    public virtual User? RequestedByUser { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
