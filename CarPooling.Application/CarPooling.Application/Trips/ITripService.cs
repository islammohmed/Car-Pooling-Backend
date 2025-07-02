using CarPooling.Application.Trips.DTOs;

namespace CarPooling.Application.Trips
{
    public interface ITripService
    {
        Task<PaginatedResponse<TripListDto>> GetAllTripsAsync(PaginationParams paginationParams);
        Task<int> CreateTripAsync(CreateTripDto tripDto);
    }
} 