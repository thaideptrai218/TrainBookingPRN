using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? IdcardNumber { get; set; }

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool IsGuestAccount { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();

    public virtual ICollection<Refund> RefundProcessedByUsers { get; set; } = new List<Refund>();

    public virtual ICollection<Refund> RefundRequestedByUsers { get; set; } = new List<Refund>();

    public virtual ICollection<TemporarySeatHold> TemporarySeatHolds { get; set; } = new List<TemporarySeatHold>();
}
