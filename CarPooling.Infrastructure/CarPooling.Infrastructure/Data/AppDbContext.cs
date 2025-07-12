using Microsoft.EntityFrameworkCore;
using CarPooling.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CarPooling.Domain.Enums;

namespace CarPooling.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        public DbSet<Trip> Trips { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<TripParticipant> TripParticipants { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<DeliveryRequest> DeliveryRequests { get; set; }
        public DbSet<DocumentVerification> DocumentVerifications { get; set; }
        public new DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure enum storage
            modelBuilder.Entity<User>()
                .Property(e => e.UserRole)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(e => e.Gender)
                .HasConversion<string>();

            modelBuilder.Entity<Trip>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<TripParticipant>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<DocumentVerification>()
                .Property(e => e.VerificationStatus)
                .HasConversion<string>();

            // Configure relationships
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany()
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TripParticipant>()
                .HasOne(tp => tp.User)
                .WithMany()
                .HasForeignKey(tp => tp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TripParticipant>()
                .HasOne(tp => tp.Trip)
                .WithMany(t => t.Participants)
                .HasForeignKey(tp => tp.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Sender)
                .WithMany()
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Receiver)
                .WithMany()
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Trip)
                .WithMany()
                .HasForeignKey(f => f.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DeliveryRequest>()
                .HasOne(dr => dr.Sender)
                .WithMany()
                .HasForeignKey(dr => dr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<DeliveryRequest>()
                .HasOne(dr => dr.Trip)
                .WithMany()
                .HasForeignKey(dr => dr.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DocumentVerification>()
                .HasOne(dv => dv.User)
                .WithMany()
                .HasForeignKey(dv => dv.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DocumentVerification>()
                .HasOne(dv => dv.Admin)
                .WithMany()
                .HasForeignKey(dv => dv.AdminId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Sender)
                .WithMany()
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Receiver)
                .WithMany()
                .HasForeignKey(c => c.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Trip)
                .WithMany()
                .HasForeignKey(c => c.TripId)
                .OnDelete(DeleteBehavior.NoAction);


            // Fix DeliveryRequests potential cascade issue
            modelBuilder.Entity<DeliveryRequest>()
                .HasOne(dr => dr.Trip)
                .WithMany(t => t.Deliveries)
                .HasForeignKey(dr => dr.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            // Set decimal precision for all decimal properties
            modelBuilder.Entity<DeliveryRequest>()
                .Property(d => d.Price)
                .HasPrecision(18, 2);



            modelBuilder.Entity<Trip>()
                .Property(t => t.PricePerSeat)
                .HasPrecision(18, 2);
        }
    }
}