using CarPooling.Domain.Entities;


namespace CarPooling.Domain.Repositories
{
    public interface ITripRepository
    {
        Task<int> create(Trip trip);
        Task<Trip?> GetTripWithParticipants(int tripId);
        Task UpdateTripAsync(Trip trip);
        Task<bool> UserExists(string userId);
        Task<string> GetUserGender(string userId);




    }
}
