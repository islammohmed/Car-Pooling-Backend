using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using AutoMapper;
using CarPooling.Domain.Repositories;
using FluentValidation;


namespace CarPooling.Application.Trips
{
    public class BookTripService(
        ITripRepository tripRepository,
        IMapper mapper,
        IValidator<BookTripDto> validator) : IBookTripService
    {
        public async Task<TripParticipantDto> BookTripAsync(BookTripDto request)
        {
            // Validate request using FluentValidation
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Invalid booking request: {errors}");
            }

            bool userExists = await tripRepository.UserExists(request.UserId);
            if (!userExists)
                throw new Exception("User account not found. Please verify your account information.");

            var trip = await tripRepository.GetTripWithParticipants(request.TripId);

            if (trip == null)
                throw new Exception("Trip not found.");

            if (trip.AvailableSeats < request.SeatCount)
                throw new Exception($"Not enough seats available. Only {trip.AvailableSeats} seats left.");

            trip.Participants ??= new List<TripParticipant>();
            var existing = trip.Participants
                .FirstOrDefault(tp => tp.UserId == request.UserId);

            if (existing != null)
                throw new Exception("You have already booked this trip.");

            var participant = new TripParticipant
            {
                TripId = request.TripId,
                UserId = request.UserId,
                SeatCount = request.SeatCount,
                Status = JoinStatus.Confirmed,
                JoinedAt = DateTime.UtcNow
            };

            trip.Participants.Add(participant);
            trip.AvailableSeats -= request.SeatCount;

            await tripRepository.UpdateTripAsync(trip);

            return mapper.Map<TripParticipantDto>(participant);
        }
    }
}