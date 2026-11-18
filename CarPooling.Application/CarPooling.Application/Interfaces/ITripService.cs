
using CarPooling.Application.Trips.DTOs;
using CarPooling.Application.DTOs;
using CarPooling.Domain.DTOs;
using CarPooling.Application.Common.Results;

namespace CarPooling.Application.Interfaces
{
    public interface ITripService
    {
        Task<Result<PaginatedResponse<TripListDto>>> GetAllTripsAsync(
            PaginationParams paginationParams);

        Task<Result<TripListDto>> GetTripByIdAsync(int tripId);

        Task<Result<int>> CreateTripAsync(CreateTripDto tripDto);

        Task<Result<IEnumerable<TripListDto>>> GetUserBookingsAsync(string userId);

        Task<Result<IEnumerable<TripListDto>>> GetUserTripsAsync(string userId);

        Task<Result<IEnumerable<TripListDto>>> SearchTripsAsync(
            string source, string destination, DateTime date);

        Task<Result<TripListDto>> CompleteTripAsync(int tripId, string driverId);

        Task<Result<bool>> UpdateTripStatusAsync(int tripId);

        Task<Result<bool>> IsUserBookedOnTripAsync(string userId, int tripId);

        Task<Result<List<TripParticipantDto>>> GetTripParticipantsAsync(int tripId);
    }
}
