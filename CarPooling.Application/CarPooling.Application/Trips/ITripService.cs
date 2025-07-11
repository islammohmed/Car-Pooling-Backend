using CarPooling.Application.Trips.DTOs;
using CarPooling.Application.DTOs;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Trips
{
    public interface ITripService
    {
        Task<PaginatedResponse<TripListDto>> GetAllTripsAsync(PaginationParams paginationParams);
        Task<TripListDto?> GetTripByIdAsync(int tripId);
        Task<int> CreateTripAsync(CreateTripDto tripDto);
        Task<IEnumerable<TripListDto>> GetUserBookingsAsync(string userId);
        Task<IEnumerable<TripListDto>> GetUserTripsAsync(string userId);
        Task<IEnumerable<TripListDto>> SearchTripsAsync(string source, string destination, DateTime date);
        Task<ApiResponse<TripListDto>> CompleteTripAsync(int tripId, string driverId);
        Task<ApiResponse<bool>> UpdateTripStatusAsync(int tripId);
        Task<ApiResponse<bool>> IsUserBookedOnTripAsync(string userId, int tripId);
        Task<List<TripParticipantDto>> GetTripParticipantsAsync(int tripId);
    }
} 