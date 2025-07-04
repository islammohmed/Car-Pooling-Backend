using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Infrastructure.Data;
using CarPooling.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CarPooling.Application.Trips.DTOs;

namespace CarPooling.Infrastructure.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly AppDbContext context;

        public TripRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<int> CreateAsync(Trip trip)
        {
            context.Trips.Add(trip);
           // await context.SaveChangesAsync();
            return trip.TripId;
        }

        public async Task<Trip?> GetByIdAsync(int id)
        {
            return await context.Trips
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.TripId == id);
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

        public async Task<(IEnumerable<Trip> Items, int TotalCount)> GetAllTripsAsync(PaginationParams paginationParams)
        {
            var query = context.Trips
                .Include(t => t.Driver)
                .Include(t => t.Participants)
                .OrderByDescending(t => t.StartTime)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Trip?> GetByIdWithParticipantsAsync(int id)
        {
            return await context.Trips
                .Include(t => t.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.TripId == id);
        }
    }
}
