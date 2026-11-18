using AutoMapper;
using CarPooling.Application.Common.Results;
using CarPooling.Application.DTOs;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.DTOs;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

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

        public async Task<Result<PaginatedResponse<TripListDto>>> GetAllTripsAsync(PaginationParams paginationParams)
        {
            var (trips, totalCount) = await _unitOfWork.Trips.GetAllTripsAsync(paginationParams.PageNumber, paginationParams.PageSize);

            var tripDtos = _mapper.Map<IEnumerable<TripListDto>>(trips);

            var response = new PaginatedResponse<TripListDto>
            {
                Items = tripDtos,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
            };

            return Result<PaginatedResponse<TripListDto>>.Success(response);
        }

        public async Task<Result<TripListDto>> GetTripByIdAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
            if (trip == null)
                return Result<TripListDto>.Failure("Trip not found");

            return Result<TripListDto>.Success(_mapper.Map<TripListDto>(trip));
        }

        public async Task<Result<int>> CreateTripAsync(CreateTripDto tripDto)
        {
            _logger.LogInformation("Validating trip creation request");

            // Validate the trip data
            var validationResult = await _validator.ValidateAsync(tripDto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return Result<int>.Failure($"Invalid trip creation request: {errors}");
            }

            _logger.LogInformation("Creating a new trip");

            // Map DTO to entity
            var trip = _mapper.Map<Trip>(tripDto);

            // Set CreatedAt to current UTC time
            trip.CreatedAt = DateTime.UtcNow;

            // Save to database
            await _unitOfWork.Trips.CreateAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            return Result<int>.Success(trip.TripId, "Trip created successfully");
        }

        public async Task<Result<IEnumerable<TripListDto>>> GetUserBookingsAsync(string userId)
        {
            // Get all trips with participants
            var trips = await _unitOfWork.Trips.GetAllTripsWithParticipantsAsync();

            // Filter trips where the user is a participant
            var userBookings = trips
                .Where(t => t.Participants != null && t.Participants.Any(p => p.UserId == userId))
                .ToList();

            return Result<IEnumerable<TripListDto>>.Success(_mapper.Map<IEnumerable<TripListDto>>(userBookings));
        }

        public async Task<Result<IEnumerable<TripListDto>>> GetUserTripsAsync(string userId)
        {
            // Get all trips
            var (trips, _) = await _unitOfWork.Trips.GetAllTripsAsync(1, 1000);

            // Filter trips where the user is the driver
            var userTrips = trips
                .Where(t => t.Driver?.Id == userId)
                .ToList();

            return Result<IEnumerable<TripListDto>>.Success(_mapper.Map<IEnumerable<TripListDto>>(userTrips));
        }

        public async Task<Result<IEnumerable<TripListDto>>> SearchTripsAsync(string source, string destination, DateTime date)
        {
            try
            {
                // Use raw SQL query to avoid the UserId1 column issue
                var trips = await _unitOfWork.Trips.SearchTripsAsync(source, destination, date);
                var tripDtos = _mapper.Map<IEnumerable<TripListDto>>(trips);

                // Make sure the destination is correctly mapped
                foreach (var tripDto in tripDtos)
                {
                    if (string.IsNullOrEmpty(tripDto.Destination))
                    {
                        // Find the corresponding trip to get the destination
                        var trip = trips.FirstOrDefault(t => t.TripId == tripDto.TripId);
                        if (trip != null)
                        {
                            tripDto.Destination = trip.Destination;
                        }
                    }
                }

                return Result<IEnumerable<TripListDto>>.Success(tripDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching trips: {Message}", ex.Message);
                return Result<IEnumerable<TripListDto>>.Failure("An error occurred while searching trips");
            }
        }

        public async Task<Result<TripListDto>> CompleteTripAsync(int tripId, string driverId)
        {
            try
            {
                // Get the trip with participants
                var trip = await _unitOfWork.Trips.GetTripWithParticipants(tripId);

                if (trip == null)
                    return Result<TripListDto>.Failure("Trip not found");

                // Verify the user is the driver of the trip
                if (trip.DriverId != driverId)
                    return Result<TripListDto>.Failure("Only the trip driver can complete the trip");

                // Check if trip can be completed
                if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
                    return Result<TripListDto>.Failure("Cannot complete a trip that is already completed or cancelled");

                // Update the trip status to completed
                trip.Status = TripStatus.Completed;

                // Save changes
                await _unitOfWork.Trips.UpdateTripAsync(trip);
                await _unitOfWork.SaveChangesAsync();

                // Return the updated trip
                return Result<TripListDto>.Success(_mapper.Map<TripListDto>(trip), "Trip completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing trip");
                return Result<TripListDto>.Failure($"Error completing trip: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateTripStatusAsync(int tripId)
        {
            try
            {
                // If tripId is 0, check all trips
                if (tripId == 0)
                {
                    var (trips, _) = await _unitOfWork.Trips.GetAllTripsAsync(1, 1000);

                    int updatedCount = 0;
                    foreach (var trip in trips)
                    {
                        bool updated = await UpdateSingleTripStatusAsync(trip);
                        if (updated) updatedCount++;
                    }

                    return Result<bool>.Success(updatedCount > 0,
                        updatedCount > 0 ? $"Updated {updatedCount} trips" : "No trips needed updating");
                }
                else
                {
                    // Get the trip with participants
                    var trip = await _unitOfWork.Trips.GetTripWithParticipants(tripId);

                    if (trip == null)
                        return Result<bool>.Failure("Trip not found");

                    bool updated = await UpdateSingleTripStatusAsync(trip);
                    return Result<bool>.Success(updated,
                        updated ? "Trip status updated" : "No status update needed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip status");
                return Result<bool>.Failure("An error occurred while updating trip status");
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

        public async Task<Result<bool>> IsUserBookedOnTripAsync(string userId, int tripId)
        {
            try
            {
                // Get the trip with participants
                var trip = await _unitOfWork.Trips.GetTripWithParticipants(tripId);

                if (trip == null)
                    return Result<bool>.Failure("Trip not found");

                // Check if the user is the driver
                bool isDriver = trip.DriverId == userId;

                // Check if the user is a participant
                bool isParticipant = trip.Participants != null &&
                                    trip.Participants.Any(p => p.UserId == userId &&
                                                             p.Status != JoinStatus.Cancelled);

                // Provide more detailed information in the response
                if (isDriver)
                {
                    return Result<bool>.Success(true, "You are the driver of this trip");
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
                    return Result<bool>.Success(true, statusMessage);
                }
                else
                {
                    return Result<bool>.Success(false, "You have not booked this trip");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user booking");
                return Result<bool>.Failure("An error occurred while checking the user booking");
            }
        }

        public async Task<Result<List<TripParticipantDto>>> GetTripParticipantsAsync(int tripId)
        {
            // Get the trip with participants and their user details
            var trip = await _unitOfWork.Trips.GetByIdWithParticipantsAsync(tripId);

            if (trip == null)
                return Result<List<TripParticipantDto>>.Failure("Trip not found");

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

            return Result<List<TripParticipantDto>>.Success(participants);
        }
    }
}