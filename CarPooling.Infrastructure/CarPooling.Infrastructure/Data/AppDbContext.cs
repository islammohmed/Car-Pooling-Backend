using Microsoft.EntityFrameworkCore;
using CarPooling.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CarPooling.Data
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



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enum to string conversion
            modelBuilder.Entity<User>()
                .Property(u => u.UserRole)
                .HasConversion<string>();

            modelBuilder.Entity<Trip>()
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<TripParticipant>()
                .Property(tp => tp.Status)
                .HasConversion<string>();

            // Composite Key for TripParticipant
            modelBuilder.Entity<TripParticipant>()
                .HasKey(tp => new { tp.TripId, tp.UserId });

            // Trip-Driver (User) relationship
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany()
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Car-Driver (User) relationship
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Driver)
                .WithMany(u => u.Cars)
                .HasForeignKey(c => c.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // TripParticipant → Trip
            modelBuilder.Entity<TripParticipant>()
                .HasOne(tp => tp.Trip)
                .WithMany(t => t.Participants)
                .HasForeignKey(tp => tp.TripId);

            // TripParticipant → User
            modelBuilder.Entity<TripParticipant>()
                .HasOne(tp => tp.User)
                .WithMany(u => u.TripParticipations)
                .HasForeignKey(tp => tp.UserId);

            modelBuilder.Entity<DocumentVerification>()
              .HasOne(dv => dv.User)
               .WithMany()
             .HasForeignKey(dv => dv.User_ID)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentVerification>()
                .HasOne(dv => dv.Admin)
                .WithMany()
                .HasForeignKey(dv => dv.Verified_By_Admin_ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
    .HasOne(c => c.Sender)
    .WithMany()
    .HasForeignKey(c => c.Sender_ID)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Receiver)
                .WithMany()
                .HasForeignKey(c => c.Receiver_ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Feedback>()
    .HasOne(f => f.Sender)
    .WithMany()
    .HasForeignKey(f => f.Sender_User_ID)
    .OnDelete(DeleteBehavior.Restrict); // Prevents multiple cascade paths

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Receiver)
                .WithMany()
                .HasForeignKey(f => f.Receiver_User_ID)
                .OnDelete(DeleteBehavior.Cascade); // Only one can cascade

            modelBuilder.Entity<Payment>()
    .HasOne(p => p.Payer)
    .WithMany()
    .HasForeignKey(p => p.Payer_ID)
    .OnDelete(DeleteBehavior.Restrict); // Prevents multiple cascade paths

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Receiver)
                .WithMany()
                .HasForeignKey(p => p.Receiver_User_ID)
                .OnDelete(DeleteBehavior.Cascade); // Only one can cascade



            // Explicitly set decimal precision for monetary values
            modelBuilder.Entity<Trip>()
                .Property(t => t.PricePerSeat)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DeliveryRequest>()
                .Property(d => d.price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
    }
}
