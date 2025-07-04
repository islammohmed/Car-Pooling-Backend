using CarPooling.Application.DTOs;
using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Interfaces
{
    public interface IDeliveryService
    {
        Task<ApiResponse<DeliveryRequestResponseDto>> CreateRequestAsync(string userId, CreateDeliveryRequestDto requestDto);
        Task<ApiResponse<DeliveryRequestResponseDto>> GetRequestByIdAsync(int id);
        Task<ApiResponse<List<DeliveryRequestResponseDto>>> GetPendingRequestsAsync();
        Task<ApiResponse<List<DeliveryRequestResponseDto>>> GetUserRequestsAsync(string userId);
        Task<ApiResponse<List<DeliveryRequestResponseDto>>> GetDriverDeliveriesAsync(string driverId);
        Task<ApiResponse<DeliveryRequestResponseDto>> AcceptDeliveryAsync(string driverId, int requestId, int tripId);
        Task<ApiResponse<DeliveryRequestResponseDto>> UpdateDeliveryStatusAsync(string driverId, int requestId, UpdateDeliveryStatusDto updateDto);
        Task<ApiResponse<bool>> CancelRequestAsync(string userId, int requestId);
        Task<ApiResponse<List<TripListDto>>> GetMatchingTripsAsync(int requestId);
    }
} 