using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TrainBookingApp.Models;

public partial class Context : DbContext
{
    public Context()
    {
    }

    public Context(DbContextOptions<Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<CancellationPolicy> CancellationPolicies { get; set; }

    public virtual DbSet<Coach> Coaches { get; set; }

    public virtual DbSet<CoachType> CoachTypes { get; set; }

    public virtual DbSet<Passenger> Passengers { get; set; }

    public virtual DbSet<PassengerType> PassengerTypes { get; set; }

    public virtual DbSet<PricingRule> PricingRules { get; set; }

    public virtual DbSet<Refund> Refunds { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<RouteStation> RouteStations { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SeatType> SeatTypes { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<TemporarySeatHold> TemporarySeatHolds { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Train> Trains { get; set; }

    public virtual DbSet<TrainType> TrainTypes { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<TripStation> TripStations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=PRN212_TrainBookingSystem;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(e => e.BookingCode, "UQ_Bookings_BookingCode").IsUnique();

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.BookingCode).HasMaxLength(50);
            entity.Property(e => e.BookingStatus).HasMaxLength(20);
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.Source).HasMaxLength(50);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_UserID");
        });

        modelBuilder.Entity<CancellationPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId);

            entity.Property(e => e.PolicyId).HasColumnName("PolicyID");
            entity.Property(e => e.FeePercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.FixedFeeAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.HoursBeforeDepartureMax).HasColumnName("HoursBeforeDeparture_Max");
            entity.Property(e => e.HoursBeforeDepartureMin).HasColumnName("HoursBeforeDeparture_Min");
            entity.Property(e => e.PolicyName).HasMaxLength(150);
        });

        modelBuilder.Entity<Coach>(entity =>
        {
            entity.HasIndex(e => new { e.TrainId, e.CoachNumber }, "UQ_Coaches_TrainCoachNumber").IsUnique();

            entity.HasIndex(e => new { e.TrainId, e.PositionInTrain }, "UQ_Coaches_TrainPosition").IsUnique();

            entity.Property(e => e.CoachId).HasColumnName("CoachID");
            entity.Property(e => e.CoachName).HasMaxLength(50);
            entity.Property(e => e.CoachTypeId).HasColumnName("CoachTypeID");
            entity.Property(e => e.TrainId).HasColumnName("TrainID");

            entity.HasOne(d => d.CoachType).WithMany(p => p.Coaches)
                .HasForeignKey(d => d.CoachTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Coaches_CoachTypeID");

            entity.HasOne(d => d.Train).WithMany(p => p.Coaches)
                .HasForeignKey(d => d.TrainId)
                .HasConstraintName("FK_Coaches_TrainID");
        });

        modelBuilder.Entity<CoachType>(entity =>
        {
            entity.HasIndex(e => e.TypeName, "UQ_CoachTypes_TypeName").IsUnique();

            entity.Property(e => e.CoachTypeId).HasColumnName("CoachTypeID");
            entity.Property(e => e.PriceMultiplier).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.Property(e => e.PassengerId).HasColumnName("PassengerID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IdcardNumber)
                .HasMaxLength(20)
                .HasColumnName("IDCardNumber");
            entity.Property(e => e.PassengerTypeId).HasColumnName("PassengerTypeID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.PassengerType).WithMany(p => p.Passengers)
                .HasForeignKey(d => d.PassengerTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Passengers_PassengerTypeID");

            entity.HasOne(d => d.User).WithMany(p => p.Passengers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Passengers_UserID");
        });

        modelBuilder.Entity<PassengerType>(entity =>
        {
            entity.HasIndex(e => e.TypeName, "UQ_PassengerTypes_TypeName").IsUnique();

            entity.Property(e => e.PassengerTypeId).HasColumnName("PassengerTypeID");
            entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<PricingRule>(entity =>
        {
            entity.HasKey(e => e.RuleId);

            entity.Property(e => e.RuleId).HasColumnName("RuleID");
            entity.Property(e => e.BasePricePerKm).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.RuleName).HasMaxLength(150);
            entity.Property(e => e.TrainTypeId).HasColumnName("TrainTypeID");

            entity.HasOne(d => d.Route).WithMany(p => p.PricingRules)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("FK_PricingRules_RouteID");

            entity.HasOne(d => d.TrainType).WithMany(p => p.PricingRules)
                .HasForeignKey(d => d.TrainTypeId)
                .HasConstraintName("FK_PricingRules_TrainTypeID");
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.HasIndex(e => e.TicketId, "UQ_Refunds_TicketID").IsUnique();

            entity.Property(e => e.RefundId).HasColumnName("RefundID");
            entity.Property(e => e.ActualRefundAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.AppliedPolicyId).HasColumnName("AppliedPolicyID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.FeeAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OriginalTicketPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProcessedByUserId).HasColumnName("ProcessedByUserID");
            entity.Property(e => e.RefundMethod).HasMaxLength(50);
            entity.Property(e => e.RefundTransactionId)
                .HasMaxLength(100)
                .HasColumnName("RefundTransactionID");
            entity.Property(e => e.RequestedByUserId).HasColumnName("RequestedByUserID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TicketId).HasColumnName("TicketID");

            entity.HasOne(d => d.AppliedPolicy).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.AppliedPolicyId)
                .HasConstraintName("FK_Refunds_AppliedPolicyID");

            entity.HasOne(d => d.Booking).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Refunds_BookingID");

            entity.HasOne(d => d.ProcessedByUser).WithMany(p => p.RefundProcessedByUsers)
                .HasForeignKey(d => d.ProcessedByUserId)
                .HasConstraintName("FK_Refunds_ProcessedByUserID");

            entity.HasOne(d => d.RequestedByUser).WithMany(p => p.RefundRequestedByUsers)
                .HasForeignKey(d => d.RequestedByUserId)
                .HasConstraintName("FK_Refunds_RequestedByUserID");

            entity.HasOne(d => d.Ticket).WithOne(p => p.Refund)
                .HasForeignKey<Refund>(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Refunds_TicketID");
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasIndex(e => e.RouteName, "UQ_Routes_RouteName").IsUnique();

            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.RouteName).HasMaxLength(100);
        });

        modelBuilder.Entity<RouteStation>(entity =>
        {
            entity.HasIndex(e => new { e.RouteId, e.SequenceNumber }, "UQ_RouteStations_RouteSequence").IsUnique();

            entity.HasIndex(e => new { e.RouteId, e.StationId }, "UQ_RouteStations_RouteStation").IsUnique();

            entity.Property(e => e.RouteStationId).HasColumnName("RouteStationID");
            entity.Property(e => e.DistanceFromStart).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.StationId).HasColumnName("StationID");

            entity.HasOne(d => d.Route).WithMany(p => p.RouteStations)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("FK_RouteStations_RouteID");

            entity.HasOne(d => d.Station).WithMany(p => p.RouteStations)
                .HasForeignKey(d => d.StationId)
                .HasConstraintName("FK_RouteStations_StationID");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasIndex(e => new { e.CoachId, e.SeatName }, "UQ_Seats_CoachSeatName").IsUnique();

            entity.HasIndex(e => new { e.CoachId, e.SeatNumber }, "UQ_Seats_CoachSeatNumber").IsUnique();

            entity.Property(e => e.SeatId).HasColumnName("SeatID");
            entity.Property(e => e.CoachId).HasColumnName("CoachID");
            entity.Property(e => e.SeatName).HasMaxLength(50);
            entity.Property(e => e.SeatTypeId).HasColumnName("SeatTypeID");

            entity.HasOne(d => d.Coach).WithMany(p => p.Seats)
                .HasForeignKey(d => d.CoachId)
                .HasConstraintName("FK_Seats_CoachID");

            entity.HasOne(d => d.SeatType).WithMany(p => p.Seats)
                .HasForeignKey(d => d.SeatTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seats_SeatTypeID");
        });

        modelBuilder.Entity<SeatType>(entity =>
        {
            entity.HasIndex(e => e.TypeName, "UQ_SeatTypes_TypeName").IsUnique();

            entity.Property(e => e.SeatTypeId).HasColumnName("SeatTypeID");
            entity.Property(e => e.PriceMultiplier).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasIndex(e => e.StationCode, "UQ_Stations_StationCode").IsUnique();

            entity.HasIndex(e => new { e.StationName, e.City }, "UQ_Stations_StationName_City").IsUnique();

            entity.Property(e => e.StationId).HasColumnName("StationID");
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);
            entity.Property(e => e.Region).HasMaxLength(50);
            entity.Property(e => e.StationCode).HasMaxLength(20);
            entity.Property(e => e.StationName).HasMaxLength(100);
        });

        modelBuilder.Entity<TemporarySeatHold>(entity =>
        {
            entity.HasKey(e => e.HoldId);

            entity.Property(e => e.HoldId).HasColumnName("HoldID");
            entity.Property(e => e.CoachId).HasColumnName("CoachID");
            entity.Property(e => e.LegDestinationStationId).HasColumnName("legDestinationStationId");
            entity.Property(e => e.LegOriginStationId).HasColumnName("legOriginStationId");
            entity.Property(e => e.SeatId).HasColumnName("SeatID");
            entity.Property(e => e.SessionId)
                .HasMaxLength(100)
                .HasColumnName("SessionID");
            entity.Property(e => e.TripId).HasColumnName("TripID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Coach).WithMany(p => p.TemporarySeatHolds)
                .HasForeignKey(d => d.CoachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TemporarySeatHolds_CoachID");

            entity.HasOne(d => d.LegDestinationStation).WithMany(p => p.TemporarySeatHoldLegDestinationStations)
                .HasForeignKey(d => d.LegDestinationStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TemporarySeatHolds_DestinationStationID");

            entity.HasOne(d => d.LegOriginStation).WithMany(p => p.TemporarySeatHoldLegOriginStations)
                .HasForeignKey(d => d.LegOriginStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TemporarySeatHolds_OriginStationID");

            entity.HasOne(d => d.Seat).WithMany(p => p.TemporarySeatHolds)
                .HasForeignKey(d => d.SeatId)
                .HasConstraintName("FK_TemporarySeatHolds_SeatID");

            entity.HasOne(d => d.Trip).WithMany(p => p.TemporarySeatHolds)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("FK_TemporarySeatHolds_TripID");

            entity.HasOne(d => d.User).WithMany(p => p.TemporarySeatHolds)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_TemporarySeatHolds_UserID");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasIndex(e => e.TicketCode, "UQ_Tickets_TicketCode").IsUnique();

            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.CoachNameSnapshot).HasMaxLength(50);
            entity.Property(e => e.EndStationId).HasColumnName("EndStationID");
            entity.Property(e => e.ParentTicketId).HasColumnName("ParentTicketID");
            entity.Property(e => e.PassengerId).HasColumnName("PassengerID");
            entity.Property(e => e.PassengerIdcardNumberSnapshot)
                .HasMaxLength(20)
                .HasColumnName("PassengerIDCardNumberSnapshot");
            entity.Property(e => e.PassengerNameSnapshot).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SeatId).HasColumnName("SeatID");
            entity.Property(e => e.SeatNameSnapshot).HasMaxLength(50);
            entity.Property(e => e.StartStationId).HasColumnName("StartStationID");
            entity.Property(e => e.TicketCode).HasMaxLength(50);
            entity.Property(e => e.TicketStatus).HasMaxLength(20);
            entity.Property(e => e.TripId).HasColumnName("TripID");

            entity.HasOne(d => d.Booking).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Tickets_BookingID");

            entity.HasOne(d => d.EndStation).WithMany(p => p.TicketEndStations)
                .HasForeignKey(d => d.EndStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_EndStationID");

            entity.HasOne(d => d.ParentTicket).WithMany(p => p.InverseParentTicket)
                .HasForeignKey(d => d.ParentTicketId)
                .HasConstraintName("FK_Tickets_ParentTicketID");

            entity.HasOne(d => d.Passenger).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.PassengerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_PassengerID");

            entity.HasOne(d => d.Seat).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_SeatID");

            entity.HasOne(d => d.StartStation).WithMany(p => p.TicketStartStations)
                .HasForeignKey(d => d.StartStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_StartStationID");

            entity.HasOne(d => d.Trip).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_TripID");
        });

        modelBuilder.Entity<Train>(entity =>
        {
            entity.HasIndex(e => e.TrainName, "UQ_Trains_TrainName").IsUnique();

            entity.Property(e => e.TrainId).HasColumnName("TrainID");
            entity.Property(e => e.TrainName).HasMaxLength(50);
            entity.Property(e => e.TrainTypeId).HasColumnName("TrainTypeID");

            entity.HasOne(d => d.TrainType).WithMany(p => p.Trains)
                .HasForeignKey(d => d.TrainTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trains_TrainTypeID");
        });

        modelBuilder.Entity<TrainType>(entity =>
        {
            entity.HasIndex(e => e.TypeName, "UQ_TrainTypes_TypeName").IsUnique();

            entity.Property(e => e.TrainTypeId).HasColumnName("TrainTypeID");
            entity.Property(e => e.AverageVelocity).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.Property(e => e.TripId).HasColumnName("TripID");
            entity.Property(e => e.BasePriceMultiplier).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.TrainId).HasColumnName("TrainID");
            entity.Property(e => e.TripStatus).HasMaxLength(20);

            entity.HasOne(d => d.Route).WithMany(p => p.Trips)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trips_RouteID");

            entity.HasOne(d => d.Train).WithMany(p => p.Trips)
                .HasForeignKey(d => d.TrainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trips_TrainID");
        });

        modelBuilder.Entity<TripStation>(entity =>
        {
            entity.HasIndex(e => new { e.TripId, e.SequenceNumber }, "UQ_TripStations_TripSequence").IsUnique();

            entity.HasIndex(e => new { e.TripId, e.StationId }, "UQ_TripStations_TripStation").IsUnique();

            entity.Property(e => e.TripStationId).HasColumnName("TripStationID");
            entity.Property(e => e.StationId).HasColumnName("StationID");
            entity.Property(e => e.TripId).HasColumnName("TripID");

            entity.HasOne(d => d.Station).WithMany(p => p.TripStations)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TripStations_StationID");

            entity.HasOne(d => d.Trip).WithMany(p => p.TripStations)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("FK_TripStations_TripID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.IdcardNumber)
                .HasMaxLength(20)
                .HasColumnName("IDCardNumber");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
