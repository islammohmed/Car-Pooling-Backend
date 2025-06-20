
using CarPooling.Application.Trips.DTOs;

namespace CarPooling.Application.Trips
{
    public interface IBookTripService
    {
        Task<TripParticipantDto> BookTripAsync(BookTripDto request);
    }
}
