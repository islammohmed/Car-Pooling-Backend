using Car_Pooling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Car_Pooling.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {

        public AppDbContext (DbContextOptions<AppDbContext> options)
           : base (options)
        {
        }

        // Your custom DbSets
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<TripParticipant> TripParticipants { get; set; }



        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            base.OnModelCreating (modelBuilder);

            // Enum to string conversion
            modelBuilder.Entity<User> ()
                .Property (u => u.UserRole)
                .HasConversion<string> ();

            modelBuilder.Entity<Trip> ()
                .Property (t => t.Status)
                .HasConversion<string> ();

            modelBuilder.Entity<TripParticipant> ()
                .Property (tp => tp.Status)
                .HasConversion<string> ();

            // Composite Key for TripParticipant
            modelBuilder.Entity<TripParticipant> ()
                .HasKey (tp => new { tp.TripId, tp.UserId });

            // Trip-Driver (User) relationship
            modelBuilder.Entity<Trip> ()
                .HasOne (t => t.Driver)
                .WithMany ()
                .HasForeignKey (t => t.DriverId)
                .OnDelete (DeleteBehavior.Restrict);

            // Car-Driver (User) relationship
            modelBuilder.Entity<Car> ()
                .HasOne (c => c.Driver)
                .WithMany (u => u.Cars)
                .HasForeignKey (c => c.DriverId)
                .OnDelete (DeleteBehavior.Restrict);

            // TripParticipant → Trip
            modelBuilder.Entity<TripParticipant> ()
                .HasOne (tp => tp.Trip)
                .WithMany (t => t.Participants)
                .HasForeignKey (tp => tp.TripId);

            // TripParticipant → User
            modelBuilder.Entity<TripParticipant> ()
                .HasOne (tp => tp.User)
                .WithMany (u => u.TripParticipations)
                .HasForeignKey (tp => tp.UserId);
        }
    }
}
