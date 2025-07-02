using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.Exceptions;
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

        public BookTripService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<BookTripDto> validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
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

            // Get the trip with its participants
            var trip = await _unitOfWork.Trips.GetTripWithParticipants(request.TripId);
            if (trip == null)
                throw TripBookingException.TripNotFound();

            // Check if there are enough seats available
            if (trip.AvailableSeats < request.SeatCount)
                throw TripBookingException.InsufficientSeats(trip.AvailableSeats);

            trip.Participants ??= new List<TripParticipant>();

            // Check if the user has already booked this trip
            var existing = trip.Participants.FirstOrDefault(tp => tp.UserId == request.UserId);
            if (existing != null)
                throw TripBookingException.AlreadyBooked();

            // Create a new trip participant
            var participant = new TripParticipant
            {
                TripId = request.TripId,
                UserId = request.UserId,
                SeatCount = request.SeatCount,
                Status = JoinStatus.Confirmed,
                JoinedAt = DateTime.UtcNow
            };

            // Update the trip
            trip.Participants.Add(participant);
            trip.AvailableSeats -= request.SeatCount;

            // Save changes
            await _unitOfWork.Trips.UpdateTripAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            // Return the mapped participant DTO
            return _mapper.Map<TripParticipantDto>(participant);
        }

        async Task<bool> IBookTripService.CancelTrip(CancelTripDto request) 
        {
            // Check if the user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
                throw TripBookingException.UserNotFound();

            // Get the trip with its participants
            var trip = await _unitOfWork.Trips.GetTripWithParticipants(request.TripId);
            if (trip == null)
                throw TripBookingException.TripNotFound();

            // Check if the user has already booked this trip
            var participant = trip.Participants?.FirstOrDefault(tp => tp.UserId == request.UserId);
            if (participant == null)
                throw TripBookingException.BookingNotFound();

            // Check if the booking is already cancelled
            if (participant.Status == JoinStatus.Cancelled)
                throw TripBookingException.AlreadyCancelled();

            participant.Status = JoinStatus.Cancelled;
            trip.AvailableSeats += participant.SeatCount;

            // Save changes
            await _unitOfWork.Trips.UpdateTripAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}