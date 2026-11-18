using CarPooling.Application.Common.Results;
using CarPooling.Application.DTOs;
using CarPooling.Application.Trips.DTOs;

namespace CarPooling.Application.Interfaces
{
    public interface IDeliveryService
    {
        Task<Result<DeliveryRequestResponseDto>> CreateRequestAsync(string userId, CreateDeliveryRequestDto requestDto);
        Task<Result<DeliveryRequestResponseDto>> GetRequestByIdAsync(int id);
        Task<Result<List<DeliveryRequestResponseDto>>> GetPendingRequestsAsync();
        Task<Result<List<DeliveryRequestResponseDto>>> GetUserRequestsAsync(string userId);
        Task<Result<List<DeliveryRequestResponseDto>>> GetDriverDeliveriesAsync(string driverId);
        Task<Result<List<DeliveryRequestResponseDto>>> GetSelectedDeliveriesForDriverAsync(string driverId);
        Task<Result<DeliveryRequestResponseDto>> AcceptDeliveryAsync(string driverId, int requestId, int tripId);
        Task<Result<DeliveryRequestResponseDto>> RejectDeliveryAsync(string driverId, int requestId);
        Task<Result<DeliveryRequestResponseDto>> UpdateDeliveryStatusAsync(string driverId, int requestId, UpdateDeliveryStatusDto updateDto);
        Task<Result<bool>> CancelRequestAsync(string userId, int requestId);
        Task<Result<List<TripListDto>>> GetMatchingTripsAsync(int requestId);
        Task<Result<int>> HandleExpiredRequestsAsync();
        Task<Result<DeliveryRequestResponseDto>> SelectTripForDeliveryAsync(string userId, int requestId, SelectTripForDeliveryDto selectTripDto);
    }
}
