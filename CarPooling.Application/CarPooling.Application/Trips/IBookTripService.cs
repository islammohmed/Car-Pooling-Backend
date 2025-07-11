
using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Trips
{
    public interface IBookTripService
    {
        Task<TripParticipantDto> BookTrip(BookTripDto request);
         Task<bool> CancelTrip(CancelTripDto request);
        Task<ApiResponse<bool>> CanDriverBookTripAsync(string driverId, int tripId);
    }
}
