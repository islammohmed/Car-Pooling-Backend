using Car_Pooling.Data;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarPooling.Infrastructure.Repositories
{
    internal class TripRepository(AppDbContext context) : ITripRepository
    {
        public async Task<int> create(Trip trip)
        {
            context.Trips.Add(trip);
            await context.SaveChangesAsync();
            return trip.TripId;
        }

        public async Task<Trip?> GetTripWithParticipants(int tripId)
        {
            return await context.Trips
                .Include(t => t.Participants)
                .FirstOrDefaultAsync(t => t.TripId == tripId);
        }

        public async Task UpdateTripAsync(Trip trip)
        {
            context.Trips.Update(trip);
            await context.SaveChangesAsync();
        }
       


    }
}
