using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.Exceptions;
using AutoMapper;
using FluentValidation;
using CarPooling.Application.Repositories;

namespace CarPooling.Application.Trips
{
    public class BookTripService : IBookTripService
    {
        private readonly ITripRepository _tripRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<BookTripDto> _validator;

        public BookTripService(
            ITripRepository tripRepository,
            IMapper mapper,
            IValidator<BookTripDto> validator)
        {
            _tripRepository = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<TripParticipantDto> BookTripAsync(BookTripDto request)
        {
            // Validate request using FluentValidation
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Invalid booking request: {errors}");
            }

            // Check if the user exists
            bool userExists = await _tripRepository.UserExists(request.UserId);
            if (!userExists)
                throw TripBookingException.UserNotFound();

            // Get the trip with its participants
            var trip = await _tripRepository.GetTripWithParticipants(request.TripId);
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
            await _tripRepository.UpdateTripAsync(trip);

            // Return the mapped participant DTO
            return _mapper.Map<TripParticipantDto>(participant);
        }
    }
}