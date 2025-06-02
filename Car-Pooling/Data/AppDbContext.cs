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
        // public DbSet<Car> Cars { get; set; }


        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            base.OnModelCreating (modelBuilder);

            // Example: Map enums as strings
            modelBuilder.Entity<User> ()
                .Property (u => u.UserRole)
                .HasConversion<string> ();

            modelBuilder.Entity<Trip> ()
                .Property (t => t.Status)
                .HasConversion<string> ();

            //modelBuilder.Entity<TripParticipant> ()
            //    .HasKey (tp => new { tp.TripId, tp.UserId });
        }
    }
}
