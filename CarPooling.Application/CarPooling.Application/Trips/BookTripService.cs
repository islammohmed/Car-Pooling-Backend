
using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CarPooling.Domain.Repositories;

namespace CarPooling.Application.Trips
{

    public class BookTripService(ITripRepository tripRepository, IMapper mapper) : IBookTripService
    {

        public async Task<TripParticipantDto> BookTripAsync(BookTripDto request)
        {
            var trip = await tripRepository.GetTripWithParticipants(request.TripId);

            if (trip == null)
                throw new Exception("Trip not found.");

            if (trip.AvailableSeats < request.SeatCount)
                throw new Exception("Not enough available seats.");

            var existing = trip.Participants
                .FirstOrDefault(tp => tp.UserId == request.UserId);

            if (existing != null)
                throw new Exception("User already booked this trip.");

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

