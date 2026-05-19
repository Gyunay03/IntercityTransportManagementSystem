using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.ViewModels;

namespace IntercityTransportManagementSystem.Models;

public partial class IntercityTransportManagementSystemDatabaseContext : DbContext
{
    public IntercityTransportManagementSystemDatabaseContext()
    {
    }

    public IntercityTransportManagementSystemDatabaseContext(DbContextOptions<IntercityTransportManagementSystemDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bus> Buses { get; set; }

    public virtual DbSet<BusRequest> BusRequests { get; set; }

    public virtual DbSet<BusSchedule> BusSchedules { get; set; }

    public virtual DbSet<BusSeat> BusSeats { get; set; }

    public virtual DbSet<BusSeatLock> BusSeatLocks { get; set; }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<Passenger> Passengers { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Refund> Refunds { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<LiveBusPosition> LiveBusPositions { get; set; }

    public virtual DbSet<Shape> Shapes { get; set; }

    public virtual DbSet<Stop> Stops { get; set; }

    public virtual DbSet<StopTime> StopTimes { get; set; }

    public virtual DbSet<TransportLine> TransportLines { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    /*
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { 
        #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=IntercityTransportManagementSystem_Database;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False",
                x => x.UseNetTopologySuite());
        }
    }
    */

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Buses__3214EC07277431A4");

            entity.Property(e => e.RegistrationNumber).HasMaxLength(8);
        });

        modelBuilder.Entity<BusSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BusSched__3214EC0745F8EBD0");

            entity.ToTable("BusSchedule");

            entity.HasOne(d => d.Bus).WithMany(p => p.BusSchedules)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BusSchedu__BusId__4F7CD00D");

            entity.HasOne(d => d.Driver).WithMany(p => p.BusSchedules)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BusSchedu__Drive__5070F446");

            entity.HasOne(d => d.Route).WithMany(p => p.BusSchedules)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BusSchedu__Route__4E88ABD4");
        });

        modelBuilder.Entity<BusSeat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BusSeats__3214EC07106053B6");

            entity.HasOne(d => d.Bus).WithMany(p => p.BusSeats)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BusSeats__BusId__4AB81AF0");

            entity.HasIndex(s => new { s.BusId, s.Number })
                  .IsUnique();
        });

        modelBuilder.Entity<BusRequest>()
            .HasIndex(r => new { r.ScheduleId, r.Status })
            .IsUnique()
            .HasFilter("[Status] = 0");

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Drivers__3214EC07450080BE");

            entity.Property(e => e.Email).HasMaxLength(60);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Passenge__3214EC076CDDEE7A");

            entity.Property(e => e.Email).HasMaxLength(60);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC07BDC034DA");

            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Sum).HasColumnType("money");

            entity.HasOne(d => d.Passenger).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PassengerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Passen__48CFD27E");

            entity.HasOne(d => d.Reservation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReservationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Reserv__49C3F6B7");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reservat__3214EC071F7226DA");

            entity.Property(e => e.ReservationTime).HasDefaultValueSql("(getdate())");

            entity.HasIndex(r => new { r.ScheduleId, r.SeatId })
                  .HasFilter("[IsActive] = 1")
                  .IsUnique();

            entity.HasOne(d => d.Passenger).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.PassengerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__Passe__4BAC3F29");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__Sched__4CA06362");

            entity.HasOne(d => d.Seat).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__SeatI__4D94879B");

            entity.HasOne(r => r.ReturnReservation)
                  .WithMany()
                  .HasForeignKey(r => r.ReturnReservationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Refund>()
            .Property(r => r.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Routes__3214EC07D15BC429");

            entity.Property(e => e.FinalDestination).HasMaxLength(100);
            entity.Property(e => e.StartDestination).HasMaxLength(100);
            entity.Property(e => e.TicketPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC076AF395C2");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(60);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(200);
            entity.Property(e => e.Role).HasMaxLength(15);
        });

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shape>()
            .HasKey(s => new { s.ShapeId, s.Sequence });

        modelBuilder.Entity<StopTime>()
            .HasKey(st => new { st.TripId, st.StopSequence });

        modelBuilder.Entity<Stop>()
            .Property(b => b.Location)
            .HasColumnType("geography");

        modelBuilder.Entity<Shape>()
            .Property(b => b.Location)
            .HasColumnType("geography");

        modelBuilder.Entity<LiveBusPosition>()
            .Property(b => b.CurrentLocation)
            .HasColumnType("geography");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}