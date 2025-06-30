
using CarPooling.Application.Trips.DTOs;

namespace CarPooling.Application.Trips
{
    public interface IBookTripService
    {
        Task<TripParticipantDto> BookTrip(BookTripDto request);
         Task<bool> CancelTrip(CancelTripDto request);
    }
}
