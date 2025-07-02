using CarPooling.Domain.Entities;


namespace CarPooling.Application.Interfaces.Repositories
{
    public interface ITripRepository
    {
        Task<int> CreateAsync(Trip trip);
        Task<Trip?> GetTripWithParticipants(int tripId);
        Task UpdateTripAsync(Trip trip);
        Task<bool> UserExists(string userId);
        Task<string> GetUserGender(string userId);
    }
}
