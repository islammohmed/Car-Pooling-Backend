using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarPooling.Application.Interfaces;
using System.Security.Claims;
using CarPooling.Domain.DTOs;
using CarPooling.Application.DTOs;
using CarPooling.API.Extensions;

namespace CarPooling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<FeedbackResponseDto>>> CreateFeedback(CreateFeedbackDto createFeedbackDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<FeedbackResponseDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _feedbackService.CreateFeedbackAsync(userId, createFeedbackDto);
            var response = result.ToApiResponse();
            return result.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("trip/{tripId}")]
        public async Task<ActionResult<ApiResponse<List<FeedbackResponseDto>>>> GetTripFeedbacks(int tripId)
        {
            var result = await _feedbackService.GetTripFeedbacksAsync(tripId);
            var response = result.ToApiResponse();
            return result.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<UserFeedbackSummaryDto>>> GetUserFeedbackSummary(string userId)
        {
            var result = await _feedbackService.GetUserFeedbackSummaryAsync(userId);
            var response = result.ToApiResponse();
            return result.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
} 