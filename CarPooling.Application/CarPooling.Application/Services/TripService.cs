using AutoMapper;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Trips.DTOs;
using CarPooling.Application.DTOs;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.DTOs;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CarPooling.Application.Services
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateTripDto> _validator;
        private readonly ILogger<TripService> _logger;

        public TripService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            IValidator<CreateTripDto> validator,
            ILogger<TripService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<PaginatedResponse<TripListDto>> GetAllTripsAsync(PaginationParams paginationParams)
        {
            var (trips, totalCount) = await _unitOfWork.Trips.GetAllTripsAsync(paginationParams);
            
            var tripDtos = _mapper.Map<IEnumerable<TripListDto>>(trips);

            return new PaginatedResponse<TripListDto>
            {
                Items = tripDtos,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
            };
        }

        public async Task<TripListDto?> GetTripByIdAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
            return trip == null ? null : _mapper.Map<TripListDto>(trip);
        }
        
        public async Task<int> CreateTripAsync(CreateTripDto tripDto)
        {
            _logger.LogInformation("Validating trip creation request");
            
            // Validate the trip data
            var validationResult = await _validator.ValidateAsync(tripDto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Invalid trip creation request: {errors}");
            }
            
            _logger.LogInformation("Creating a new trip");
            
            // Map DTO to entity
            var trip = _mapper.Map<Trip>(tripDto);
            
            // Set CreatedAt to current UTC time
            trip.CreatedAt = DateTime.UtcNow;
            
            // Save to database
            await _unitOfWork.Trips.CreateAsync(trip);
            await _unitOfWork.SaveChangesAsync();
            
            return trip.TripId;
        }

        public async Task<IEnumerable<TripListDto>> GetUserBookingsAsync(string userId)
        {
            // Get all trips with participants
            var trips = await _unitOfWork.Trips.GetAllTripsWithParticipantsAsync();
            
            // Filter trips where the user is a participant
            var userBookings = trips
                .Where(t => t.Participants != null && t.Participants.Any(p => p.UserId == userId))
                .ToList();
            
            return _mapper.Map<IEnumerable<TripListDto>>(userBookings);
        }

        public async Task<IEnumerable<TripListDto>> GetUserTripsAsync(string userId)
        {
            // Get all trips
            var trips = await _unitOfWork.Trips.GetAllTripsAsync(new PaginationParams { PageNumber = 1, PageSize = 1000 });
            
            // Filter trips where the user is the driver
            var userTrips = trips.Items
                .Where(t => t.Driver?.Id == userId)
                .ToList();
            
            return _mapper.Map<IEnumerable<TripListDto>>(userTrips);
        }

        public async Task<IEnumerable<TripListDto>> SearchTripsAsync(string source, string destination, DateTime date)
        {
            var (trips, totalCount) = await _unitOfWork.Trips.GetAllTripsAsync(new PaginationParams { PageNumber = 1, PageSize = 1000 });

            var filteredTrips = trips
                .Where(t => t.SourceLocation.Contains(source, StringComparison.OrdinalIgnoreCase) &&
                            t.Destination.Contains(destination, StringComparison.OrdinalIgnoreCase) &&
                            t.StartTime.Date == date.Date)
                .ToList();
            
            return _mapper.Map<IEnumerable<TripListDto>>(filteredTrips);
        }

        public async Task<ApiResponse<TripListDto>> CompleteTripAsync(int tripId, string driverId)
        {
            try
            {
                // Get the trip with participants
                var trip = await _unitOfWork.Trips.GetTripWithParticipants(tripId);
                
                if (trip == null)
                    return ApiResponse<TripListDto>.ErrorResponse("Trip not found");
                
                // Verify the user is the driver of the trip
                if (trip.DriverId != driverId)
                    return ApiResponse<TripListDto>.ErrorResponse("Only the trip driver can complete the trip");
                
                // Check if trip can be completed
                if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
                    return ApiResponse<TripListDto>.ErrorResponse("Cannot complete a trip that is already completed or cancelled");
                
                // Update the trip status to completed
                trip.Status = TripStatus.Completed;
                
                // Save changes
                await _unitOfWork.Trips.UpdateTripAsync(trip);
                await _unitOfWork.SaveChangesAsync();
                
                // Return the updated trip
                return ApiResponse<TripListDto>.SuccessResponse(_mapper.Map<TripListDto>(trip));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing trip");
                return ApiResponse<TripListDto>.ErrorResponse($"Error completing trip: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UpdateTripStatusAsync(int tripId)
        {
            try
            {
                // If tripId is 0, check all trips
                if (tripId == 0)
                {
                    var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 1000 };
                    var (trips, _) = await _unitOfWork.Trips.GetAllTripsAsync(paginationParams);
                    
                    int updatedCount = 0;
                    foreach (var trip in trips)
                    {
                        bool updated = await UpdateSingleTripStatusAsync(trip);
                        if (updated) updatedCount++;
                    }
                    
                    return ApiResponse<bool>.SuccessResponse(updatedCount > 0, 
                        updatedCount > 0 ? $"Updated {updatedCount} trips" : "No trips needed updating");
                }
                else
                {
                    // Get the trip with participants
                    var trip = await _unitOfWork.Trips.GetTripWithParticipants(tripId);
                    
                    if (trip == null)
                        return ApiResponse<bool>.ErrorResponse("Trip not found");
                    
                    bool updated = await UpdateSingleTripStatusAsync(trip);
                    return ApiResponse<bool>.SuccessResponse(updated, 
                        updated ? "Trip status updated" : "No status update needed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip status");
                return ApiResponse<bool>.ErrorResponse($"Error updating trip status: {ex.Message}");
            }
        }
        
        private async Task<bool> UpdateSingleTripStatusAsync(Trip trip)
        {
            bool updated = false;
            
            // If trip is already completed or cancelled, no need to update
            if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
                return false;
            
            // Check if trip start time has passed and update to Ongoing if needed
            if ((trip.Status == TripStatus.Pending || trip.Status == TripStatus.Confirmed) && 
                trip.StartTime <= DateTime.UtcNow)
            {
                trip.Status = TripStatus.Ongoing;
                updated = true;
            }
            
            // Check if the trip is fully booked (no available seats)
            if (trip.Status == TripStatus.Pending && trip.AvailableSeats <= 0)
            {
                trip.Status = TripStatus.Confirmed;
                updated = true;
            }
            
            if (updated)
            {
                await _unitOfWork.Trips.UpdateTripAsync(trip);
                await _unitOfWork.SaveChangesAsync();
            }
            
            return updated;
        }

        public async Task<ApiResponse<bool>> IsUserBookedOnTripAsync(string userId, int tripId)
        {
            try
            {
                // Get the trip with participants
                var trip = await _unitOfWork.Trips.GetTripWithParticipants(tripId);
                
                if (trip == null)
                    return ApiResponse<bool>.ErrorResponse("Trip not found");
                
                // Check if the user is the driver
                bool isDriver = trip.DriverId == userId;
                
                // Check if the user is a participant
                bool isParticipant = trip.Participants != null && 
                                    trip.Participants.Any(p => p.UserId == userId && 
                                                             p.Status != JoinStatus.Cancelled);
                
                // Provide more detailed information in the response
                if (isDriver)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "You are the driver of this trip");
                }
                else if (isParticipant)
                {
                    var participant = trip.Participants.First(p => p.UserId == userId && p.Status != JoinStatus.Cancelled);
                    string statusMessage = participant.Status switch
                    {
                        JoinStatus.Pending => "Your booking request is pending",
                        JoinStatus.Confirmed => "You are confirmed for this trip",
                        _ => "You have booked this trip"
                    };
                    return ApiResponse<bool>.SuccessResponse(true, statusMessage);
                }
                else
                {
                    return ApiResponse<bool>.SuccessResponse(false, "You have not booked this trip");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user booking");
                return ApiResponse<bool>.ErrorResponse($"Error checking user booking: {ex.Message}");
            }
        }
        
        public async Task<List<TripParticipantDto>> GetTripParticipantsAsync(int tripId)
        {
            // Get the trip with participants and their user details
            var trip = await _unitOfWork.Trips.GetByIdWithParticipantsAsync(tripId);
            
            if (trip == null)
                return new List<TripParticipantDto>();
            
            var participants = new List<TripParticipantDto>();
            
            // Add the driver as a participant with the IsDriver flag
            if (trip.Driver != null)
            {
                participants.Add(new TripParticipantDto
                {
                    TripId = tripId,
                    UserId = trip.DriverId,
                    SeatCount = 1, // Driver takes one seat
                    Status = JoinStatus.Confirmed, // Driver is always confirmed
                    JoinedAt = trip.CreatedAt, // Driver joined when the trip was created
                    FullName = $"{trip.Driver.FirstName} {trip.Driver.LastName}",
                    PhoneNumber = trip.Driver.PhoneNumber ?? string.Empty,
                    Email = trip.Driver.Email ?? string.Empty,
                    Rating = trip.Driver.AvgRating,
                    ProfileImage = trip.Driver.NationalIdImage, // Using NationalIdImage as a substitute for profile picture
                    UserRole = trip.Driver.UserRole,
                    Gender = trip.Driver.Gender,
                    IsDriver = true
                });
            }
            
            // Add all other participants
            if (trip.Participants != null)
            {
                foreach (var participant in trip.Participants.Where(p => p.Status != JoinStatus.Cancelled))
                {
                    participants.Add(new TripParticipantDto
                    {
                        TripId = tripId,
                        UserId = participant.UserId,
                        SeatCount = participant.SeatCount,
                        Status = participant.Status,
                        JoinedAt = participant.JoinedAt,
                        FullName = $"{participant.User.FirstName} {participant.User.LastName}",
                        PhoneNumber = participant.User.PhoneNumber ?? string.Empty,
                        Email = participant.User.Email ?? string.Empty,
                        Rating = participant.User.AvgRating,
                        ProfileImage = participant.User.NationalIdImage, // Using NationalIdImage as a substitute for profile picture
                        UserRole = participant.User.UserRole,
                        Gender = participant.User.Gender,
                        IsDriver = false
                    });
                }
            }
            
            return participants;
        }
    }
} 