using CarPooling.Application.DTOs;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Interfaces
{
    public interface IFeedbackService
    {
        Task<ApiResponse<FeedbackResponseDto>> CreateFeedbackAsync(string senderId, CreateFeedbackDto createFeedbackDto);
        Task<ApiResponse<List<FeedbackResponseDto>>> GetTripFeedbacksAsync(int tripId);
        Task<ApiResponse<UserFeedbackSummaryDto>> GetUserFeedbackSummaryAsync(string userId);
    }
} 