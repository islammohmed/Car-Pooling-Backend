using CarPooling.Application.Trips.DTOs;
using CarPooling.Application.DTOs;

namespace CarPooling.Application.Trips
{
    public interface ITripService
    {
        Task<PaginatedResponse<TripListDto>> GetAllTripsAsync(PaginationParams paginationParams);
        Task<TripListDto?> GetTripByIdAsync(int tripId);
        Task<int> CreateTripAsync(CreateTripDto tripDto);
    }
} 