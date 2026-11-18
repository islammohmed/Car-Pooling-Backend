using CarPooling.Application.Common.Results;
using CarPooling.Application.DTOs;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Interfaces
{
    public interface IFeedbackService
    {
        Task<Result<FeedbackResponseDto>> CreateFeedbackAsync(string senderId, CreateFeedbackDto createFeedbackDto);
        Task<Result<List<FeedbackResponseDto>>> GetTripFeedbacksAsync(int tripId);
        Task<Result<UserFeedbackSummaryDto>> GetUserFeedbackSummaryAsync(string userId);
    }
} 