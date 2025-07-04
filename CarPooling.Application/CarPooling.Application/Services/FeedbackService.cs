using CarPooling.Application.DTOs;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITripRepository _tripRepository;

        public FeedbackService(
            IFeedbackRepository feedbackRepository,
            IUserRepository userRepository,
            ITripRepository tripRepository)
        {
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
            _tripRepository = tripRepository;
        }

        public async Task<ApiResponse<FeedbackResponseDto>> CreateFeedbackAsync(string senderId, CreateFeedbackDto createFeedbackDto)
        {
            try
            {
                // Validate sender exists
                var sender = await _userRepository.GetByIdAsync(senderId);
                if (sender == null)
                {
                    return ApiResponse<FeedbackResponseDto>.ErrorResponse("Sender not found");
                }

                // Validate receiver exists
                var receiver = await _userRepository.GetByIdAsync(createFeedbackDto.ReceiverId);
                if (receiver == null)
                {
                    return ApiResponse<FeedbackResponseDto>.ErrorResponse("Receiver not found");
                }

                // Validate trip exists and is completed
                var trip = await _tripRepository.GetByIdWithParticipantsAsync(createFeedbackDto.TripId);
                if (trip == null)
                {
                    return ApiResponse<FeedbackResponseDto>.ErrorResponse("Trip not found");
                }

                // Check if trip is completed
                if (trip.Status != TripStatus.Completed)
                {
                    return ApiResponse<FeedbackResponseDto>.ErrorResponse("Cannot give feedback for a trip that is not completed");
                }

                // Verify that both users were participants in the trip
                var senderParticipant = trip.Participants.FirstOrDefault(p => p.UserId == senderId);
                var receiverParticipant = trip.Participants.FirstOrDefault(p => p.UserId == createFeedbackDto.ReceiverId);

                if (senderParticipant == null)
                {
                    return ApiResponse<FeedbackResponseDto>.ErrorResponse("You were not a participant in this trip");
                }

                if (receiverParticipant == null)
                {
                    return ApiResponse<FeedbackResponseDto>.ErrorResponse("The receiver was not a participant in this trip");
                }

                // Verify that both participants' status was Confirmed
                if (senderParticipant.Status != JoinStatus.Confirmed || receiverParticipant.Status != JoinStatus.Confirmed)
                {
                    return ApiResponse<FeedbackResponseDto>.ErrorResponse("Both users must have been confirmed participants in the trip");
                }

                // Check if user has already given feedback for this trip
                var hasExistingFeedback = await _feedbackRepository.HasUserFeedbackForTripAsync(senderId, createFeedbackDto.TripId);
                if (hasExistingFeedback)
                {
                    return ApiResponse<FeedbackResponseDto>.ErrorResponse("You have already provided feedback for this trip");
                }

                // Validate rating range
                if (createFeedbackDto.Rating < 1 || createFeedbackDto.Rating > 5)
                {
                    return ApiResponse<FeedbackResponseDto>.ErrorResponse("Rating must be between 1 and 5");
                }

                var feedback = new Feedback
                {
                    Comment = createFeedbackDto.Comment,
                    Rating = createFeedbackDto.Rating,
                    TripId = createFeedbackDto.TripId,
                    SenderId = senderId,
                    ReceiverId = createFeedbackDto.ReceiverId
                };

                var createdFeedback = await _feedbackRepository.CreateAsync(feedback);

                // Update user's average rating
                var newAverageRating = await _feedbackRepository.GetUserAverageRatingAsync(createFeedbackDto.ReceiverId);
                receiver.AvgRating = (float)newAverageRating;
                await _userRepository.UpdateUserAsync(receiver);

                var response = new FeedbackResponseDto
                {
                    Id = createdFeedback.Id,
                    Comment = createdFeedback.Comment,
                    Rating = createdFeedback.Rating,
                    TripId = createdFeedback.TripId,
                    SenderId = createdFeedback.SenderId,
                    ReceiverId = createdFeedback.ReceiverId,
                    SenderName = $"{sender.FirstName} {sender.LastName}",
                    ReceiverName = $"{receiver.FirstName} {receiver.LastName}",
                    TripSource = trip.SourceLocation,
                    TripDestination = trip.Destination,
                    TripStartTime = trip.StartTime
                };

                return ApiResponse<FeedbackResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedbackResponseDto>.ErrorResponse($"Error creating feedback: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<FeedbackResponseDto>>> GetTripFeedbacksAsync(int tripId)
        {
            try
            {
                var feedbacks = await _feedbackRepository.GetTripFeedbacksAsync(tripId);
                var response = feedbacks.Select(f => new FeedbackResponseDto
                {
                    Id = f.Id,
                    Comment = f.Comment,
                    Rating = f.Rating,
                    TripId = f.TripId,
                    SenderId = f.SenderId,
                    ReceiverId = f.ReceiverId,
                    SenderName = $"{f.Sender.FirstName} {f.Sender.LastName}",
                    ReceiverName = $"{f.Receiver.FirstName} {f.Receiver.LastName}",
                    TripSource = f.Trip.SourceLocation,
                    TripDestination = f.Trip.Destination,
                    TripStartTime = f.Trip.StartTime
                }).ToList();

                return ApiResponse<List<FeedbackResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<FeedbackResponseDto>>.ErrorResponse($"Error getting trip feedbacks: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserFeedbackSummaryDto>> GetUserFeedbackSummaryAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserFeedbackSummaryDto>.ErrorResponse("User not found");
                }

                var feedbacks = await _feedbackRepository.GetUserReceivedFeedbacksAsync(userId);
                var averageRating = await _feedbackRepository.GetUserAverageRatingAsync(userId);
                var totalFeedbacks = await _feedbackRepository.GetUserTotalFeedbacksAsync(userId);

                var recentFeedbacks = feedbacks.Take(5).Select(f => new FeedbackResponseDto
                {
                    Id = f.Id,
                    Comment = f.Comment,
                    Rating = f.Rating,
                    TripId = f.TripId,
                    SenderId = f.SenderId,
                    ReceiverId = f.ReceiverId,
                    SenderName = $"{f.Sender.FirstName} {f.Sender.LastName}",
                    ReceiverName = $"{user.FirstName} {user.LastName}",
                    TripSource = f.Trip.SourceLocation,
                    TripDestination = f.Trip.Destination,
                    TripStartTime = f.Trip.StartTime
                }).ToList();

                var response = new UserFeedbackSummaryDto
                {
                    UserId = userId,
                    FullName = $"{user.FirstName} {user.LastName}",
                    AverageRating = averageRating,
                    TotalFeedbacks = totalFeedbacks,
                    RecentFeedbacks = recentFeedbacks
                };

                return ApiResponse<UserFeedbackSummaryDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserFeedbackSummaryDto>.ErrorResponse($"Error getting user feedback summary: {ex.Message}");
            }
        }
    }
} 