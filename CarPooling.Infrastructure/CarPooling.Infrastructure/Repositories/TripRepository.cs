using CarPooling.Domain.Interfaces.Repositories;
using CarPooling.Infrastructure.Data;
using CarPooling.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CarPooling.Application.Trips.DTOs;

namespace CarPooling.Infrastructure.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<TripRepository> _logger;

        public TripRepository(AppDbContext context, ILogger<TripRepository> logger)
        {
            this.context = context;
            _logger = logger;
        }

        public async Task<int> CreateAsync(Trip trip)
        {
            context.Trips.Add(trip);
            await context.SaveChangesAsync();
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

        public async Task<(IEnumerable<Trip> Items, int TotalCount)> GetAllTripsAsync(int pageNumber, int pageSize)
        {
            var query = context.Trips
                .Include(t => t.Driver)
                .Include(t => t.Participants)
                .OrderByDescending(t => t.StartTime)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Trip?> GetByIdWithParticipantsAsync(int id)
        {
            return await context.Trips
                .Include(t => t.Driver)
                .Include(t => t.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.TripId == id);
        }

        public async Task<IEnumerable<Trip>> GetAllTripsWithParticipantsAsync()
        {
            return await context.Trips
                .Include(t => t.Driver)
                .Include(t => t.Participants)
                    .ThenInclude(p => p.User)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<List<Trip>> GetDriverTripsAsync(string driverId)
        {
            return await context.Trips
                .Include(t => t.Driver)
                .Where(t => t.DriverId == driverId)
                .ToListAsync();
        }

        public async Task<List<Trip>> SearchTripsAsync(string source, string destination, DateTime date)
        {
            try
            {
                // Use a simpler LINQ query instead of raw SQL
                var sourceParam = source.ToLower();
                var destinationParam = destination.ToLower();
                var dateStart = date.Date;
                var dateEnd = date.Date.AddDays(1).AddSeconds(-1); // End of the day

                var trips = await context.Trips
                    .Include(t => t.Driver)
                    .Where(t => 
                        (t.SourceLocation.ToLower().Contains(sourceParam) || 
                         t.SourceCity.ToLower().Contains(sourceParam)) &&
                        (t.Destination.ToLower().Contains(destinationParam) || 
                         t.DestinationCity.ToLower().Contains(destinationParam)) &&
                        t.StartTime >= dateStart && t.StartTime <= dateEnd)
                    .OrderByDescending(t => t.StartTime)
                    .ToListAsync();

                _logger.LogInformation($"Found {trips.Count} trips matching search criteria: source={source}, destination={destination}, date={date}");
                
                return trips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchTripsAsync: {Message}", ex.Message);
                
                // If the LINQ query fails, try an even simpler approach
                var trips = await context.Trips
                    .Include(t => t.Driver)
                    .ToListAsync();
                
                // Filter in memory
                var dateStart = date.Date;
                var dateEnd = date.Date.AddDays(1).AddSeconds(-1);
                
                var filteredTrips = trips
                    .Where(t => 
                        t.StartTime.Date == date.Date &&
                        (t.SourceLocation.Contains(source, StringComparison.OrdinalIgnoreCase) ||
                         t.SourceCity.Contains(source, StringComparison.OrdinalIgnoreCase)) &&
                        (t.Destination.Contains(destination, StringComparison.OrdinalIgnoreCase) ||
                         t.DestinationCity.Contains(destination, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                
                _logger.LogInformation($"Fallback search found {filteredTrips.Count} trips");
                
                return filteredTrips;
            }
        }
    }
}
