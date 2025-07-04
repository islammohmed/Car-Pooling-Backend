using CarPooling.Domain.Entities;

namespace CarPooling.Application.Interfaces.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback> CreateAsync(Feedback feedback);
        Task<List<Feedback>> GetUserReceivedFeedbacksAsync(string userId);
        Task<List<Feedback>> GetTripFeedbacksAsync(int tripId);
        Task<bool> HasUserFeedbackForTripAsync(string senderId, int tripId);
        Task<double> GetUserAverageRatingAsync(string userId);
        Task<int> GetUserTotalFeedbacksAsync(string userId);
    }
} 