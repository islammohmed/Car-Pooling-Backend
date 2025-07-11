using CarPooling.Domain.Entities;
using CarPooling.Application.Trips.DTOs;

namespace CarPooling.Application.Interfaces.Repositories
{
    public interface ITripRepository
    {
        Task<int> CreateAsync(Trip trip);
        Task<Trip?> GetTripWithParticipants(int tripId);
        Task UpdateTripAsync(Trip trip);
        Task<bool> UserExists(string userId);
        Task<string> GetUserGender(string userId);
        Task<(IEnumerable<Trip> Items, int TotalCount)> GetAllTripsAsync(PaginationParams paginationParams);
        Task<Trip?> GetByIdAsync(int id);
        Task<Trip?> GetByIdWithParticipantsAsync(int id);
        Task<IEnumerable<Trip>> GetAllTripsWithParticipantsAsync();
        Task<List<Trip>> GetDriverTripsAsync(string driverId);
    }
}
