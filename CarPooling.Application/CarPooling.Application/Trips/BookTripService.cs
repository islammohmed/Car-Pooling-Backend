using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.Exceptions;
using CarPooling.Domain.DTOs;
using AutoMapper;
using FluentValidation;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Application.Interfaces;

namespace CarPooling.Application.Trips
{
    public class BookTripService : IBookTripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<BookTripDto> _validator;
        private readonly IValidator<CancelTripDto> _cancelValidator;

        public BookTripService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<BookTripDto> validator,
            IValidator<CancelTripDto> cancelValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _cancelValidator = cancelValidator;
        }

        public async Task<ApiResponse<bool>> CanDriverBookTripAsync(string driverId, int tripId)
        {
            try
            {
                // Get the trip
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                if (trip == null)
                    return ApiResponse<bool>.ErrorResponse("Trip not found");

                // Check if the user is the driver of this trip
                if (trip.DriverId == driverId)
                    return ApiResponse<bool>.ErrorResponse("Drivers cannot book their own trips");

                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error checking driver booking eligibility: {ex.Message}");
            }
        }

        public async Task<TripParticipantDto> BookTrip(BookTripDto request)
        {
            // Validate request using FluentValidation
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Invalid booking request: {errors}");
            }

            // Check if the user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
                throw TripBookingException.UserNotFound();

            // Check if email is confirmed
            if (!user.EmailConfirmed)
                throw TripBookingException.EmailNotConfirmed();

            // Check if user is verified
            if (!user.IsVerified)
                throw TripBookingException.UserNotVerified();

            // Get the trip with its participants
            var trip = await _unitOfWork.Trips.GetTripWithParticipants(request.TripId);
            if (trip == null)
                throw TripBookingException.TripNotFound();

            // Check if trip is still accepting bookings
            if (trip.Status != TripStatus.Pending && trip.Status != TripStatus.Confirmed)
                throw TripBookingException.TripNotAvailable();

            // Check if there are enough seats available
            if (trip.AvailableSeats < request.SeatCount)
                throw TripBookingException.InsufficientSeats(trip.AvailableSeats);

            // Check gender preference if set
            if (trip.GenderPreference.HasValue && user.Gender.HasValue && trip.GenderPreference != user.Gender)
                throw TripBookingException.GenderPreferenceMismatch();

            // If user is a driver, check if they can book this trip
            if (user.UserRole == UserRole.Driver)
            {
                if (trip.DriverId == request.UserId)
                    throw TripBookingException.DriverCannotBookOwnTrip();
            }

            trip.Participants ??= new List<TripParticipant>();

            // Check if the user has already booked this trip
            var existing = trip.Participants.FirstOrDefault(tp => tp.UserId == request.UserId);
            if (existing != null)
                throw TripBookingException.AlreadyBooked();

            // Create a new trip participant with Pending status
            var participant = new TripParticipant
            {
                TripId = request.TripId,
                UserId = request.UserId,
                SeatCount = request.SeatCount,
                Status = JoinStatus.Confirmed, // Auto-confirm the booking
                JoinedAt = DateTime.UtcNow
            };

            // Update the trip
            trip.Participants.Add(participant);
            trip.AvailableSeats -= request.SeatCount;

            // Check if trip is now full and update status if needed
            if (trip.AvailableSeats <= 0 && trip.Status == TripStatus.Pending)
            {
                trip.Status = TripStatus.Confirmed;
            }

            // Save changes
            await _unitOfWork.Trips.UpdateTripAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            // Return the mapped participant DTO
            return _mapper.Map<TripParticipantDto>(participant);
        }

        public async Task<bool> CancelTrip(CancelTripDto request)
        {
            // Validate request
            var validationResult = await _cancelValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Invalid cancellation request: {errors}");
            }

            // Get the trip with its participants
            var trip = await _unitOfWork.Trips.GetTripWithParticipants(request.TripId);
            if (trip == null)
                throw TripBookingException.TripNotFound();

            if (request.Role == UserRole.Driver)
            {
                // Verify the user is the driver of the trip
                if (trip.DriverId != request.UserId)
                    throw new ValidationException("Only the trip driver can cancel the entire trip");

                // Check if trip can be cancelled
                if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
                    throw new ValidationException("Cannot cancel a completed or already cancelled trip");

                // Cancel the entire trip
                trip.Status = TripStatus.Cancelled;

                // Cancel all pending and confirmed participant bookings
                foreach (var participant in trip.Participants)
                {
                    if (participant.Status == JoinStatus.Pending || participant.Status == JoinStatus.Confirmed)
                    {
                        participant.Status = JoinStatus.Cancelled;
                        // Here you might want to notify participants about the cancellation
                    }
                }
            }
            else // Passenger cancellation
            {
                // Find the participant booking
                var participant = trip.Participants?.FirstOrDefault(tp => tp.UserId == request.UserId);
                if (participant == null)
                    throw TripBookingException.BookingNotFound();

                // Check if the booking can be cancelled
                if (participant.Status == JoinStatus.Cancelled)
                    throw TripBookingException.AlreadyCancelled();

                if (trip.Status == TripStatus.Completed)
                    throw new ValidationException("Cannot cancel booking for a completed trip");

                // Cancel the booking
                participant.Status = JoinStatus.Cancelled;

                // Restore available seats if the booking was confirmed
                if (participant.Status == JoinStatus.Confirmed)
                {
                    trip.AvailableSeats += participant.SeatCount;
                }

                // If this was the last confirmed participant, update trip status
                if (!trip.Participants.Any(p => p.Status == JoinStatus.Confirmed))
                {
                    trip.Status = TripStatus.Pending;
                }
            }

            // Save all changes
            await _unitOfWork.Trips.UpdateTripAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}