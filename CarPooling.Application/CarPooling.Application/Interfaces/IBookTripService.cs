using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.DTOs;

using CarPooling.Application.Common.Results;

namespace CarPooling.Application.Interfaces
{
    public interface IBookTripService
    {
        Task<Result<TripParticipantDto>> BookTrip(BookTripDto request);
        Task<Result<bool>> CancelTrip(CancelTripDto request);
        Task<Result<bool>> CanDriverBookTripAsync(string driverId, int tripId);
    }
} 