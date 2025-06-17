using CarPooling.Domain.Entities;


namespace CarPooling.Domain.Repositories
{
    public interface ITripRepository
    {
        Task<int> create(Trip trip);
        Task<Trip?> GetTripWithParticipants(int tripId);
        Task UpdateTripAsync(Trip trip);

    }
}
