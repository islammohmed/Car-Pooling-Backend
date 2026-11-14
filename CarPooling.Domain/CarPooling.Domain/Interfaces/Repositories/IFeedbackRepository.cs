using CarPooling.Domain.Entities;

namespace CarPooling.Domain.Interfaces.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback> CreateAsync(Feedback feedback);
        Task<Feedback?> GetByIdAsync(int id);
        Task<IEnumerable<Feedback>> GetFeedbacksByReceiverIdAsync(string receiverId);
        Task<double> GetAverageRatingForUserAsync(string userId);
        Task<bool> HasUserFeedbackForTripAsync(string userId, int tripId);
        Task<double> GetUserAverageRatingAsync(string userId);
        Task<IEnumerable<Feedback>> GetTripFeedbacksAsync(int tripId);
        Task<IEnumerable<Feedback>> GetUserReceivedFeedbacksAsync(string userId);
        Task<int> GetUserTotalFeedbacksAsync(string userId);
    }
} 