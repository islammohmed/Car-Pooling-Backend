using CarPooling.Data;
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
            // Use transaction to ensure data consistency
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // If adding participants, validate all user IDs exist
                if (trip.Participants != null)
                {
                    foreach (var participant in trip.Participants)
                    {
                        if (participant.Id == 0) // New participant
                        {
                            bool userExists = await context.Users.AnyAsync(u => u.Id == participant.UserId);
                            if (!userExists)
                                throw new InvalidOperationException(
                                    $"User with ID {participant.UserId} does not exist.");
                        }
                    }
                }

                context.Trips.Update(trip);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Re-throw to handle at service level
            }
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
