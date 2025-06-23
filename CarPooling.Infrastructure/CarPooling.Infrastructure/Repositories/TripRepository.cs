using CarPooling.Application.Repositories;
using CarPooling.Data;
using CarPooling.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarPooling.Infrastructure.Repositories
{
    internal class TripRepository(AppDbContext context) : ITripRepository
    {
        public async Task<int> create(Trip trip)
        {
            context.Trips.Add(trip);
            await context.SaveChangesAsync();
            return trip.Id;
        }

        public async Task<Trip?> GetTripWithParticipants(int tripId)
        {
            return await context.Trips
                .Include(t => t.Participants)
                .FirstOrDefaultAsync(t => t.Id == tripId);
        }

        public async Task UpdateTripAsync(Trip trip)
        {
            context.Trips.Update(trip);
            await context.SaveChangesAsync();

        }
        public async Task<bool> UserExists(string userId)
        {
            return await context.Users.AnyAsync(u => u.Id == userId);
        }
        public async Task<string> GetUserGender(string userId)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Gender.ToString();
        }



    }
}
