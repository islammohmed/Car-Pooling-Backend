using Microsoft.EntityFrameworkCore;
using CarPooling.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DeliveryRequest> DeliveryRequests { get; set; }
        public DbSet<DocumentVerification> DocumentVerifications { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure enum storage - ADD THIS SECTION
            modelBuilder.Entity<User>()
                .Property(e => e.UserRole)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(e => e.Gender)
                .HasConversion<string>();

            // Fix for DocumentVerification - prevent multiple cascade paths
            modelBuilder.Entity<DocumentVerification>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentVerification>()
                .HasOne(d => d.Admin)
                .WithMany()
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix for other similar references with potential cascade issues
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Receiver)
                .WithMany()
                .HasForeignKey(p => p.ReceiveId)
                .OnDelete(DeleteBehavior.NoAction);

            // Fix the Payment-Trip relationship to prevent cascade paths
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Trip)
                .WithMany()
                .HasForeignKey(p => p.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            // Fix for Feedback - prevent multiple cascade paths
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

            // Fix TripParticipants potential cascade issue
            modelBuilder.Entity<TripParticipant>()
                .HasOne(tp => tp.Trip)
                .WithMany(t => t.Participants)
                .HasForeignKey(tp => tp.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            // Fix DeliveryRequests potential cascade issue
            modelBuilder.Entity<DeliveryRequest>()
                .HasOne(dr => dr.Trip)
                .WithMany()
                .HasForeignKey(dr => dr.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            // Set decimal precision for all decimal properties
            modelBuilder.Entity<DeliveryRequest>()
                .Property(d => d.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Trip>()
                .Property(t => t.PricePerSeat)
                .HasPrecision(18, 2);
        }
    }
}