using CarPooling.Domain.Entities;

namespace CarPooling.Domain.Interfaces.Repositories
{
    public interface ITripRepository
    {
        Task<int> CreateAsync(Trip trip);
        Task<Trip?> GetTripWithParticipants(int tripId);
        Task UpdateTripAsync(Trip trip);
        Task<bool> UserExists(string userId);
        Task<string> GetUserGender(string userId);
        Task<(IEnumerable<Trip> Items, int TotalCount)> GetAllTripsAsync(int pageNumber, int pageSize);
        Task<Trip?> GetByIdAsync(int id);
        Task<Trip?> GetByIdWithParticipantsAsync(int id);
        Task<IEnumerable<Trip>> GetAllTripsWithParticipantsAsync();
        Task<List<Trip>> GetDriverTripsAsync(string driverId);
        Task<List<Trip>> SearchTripsAsync(string source, string destination, DateTime date);
    }
} 