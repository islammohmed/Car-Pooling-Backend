using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;

namespace CarPooling.Domain.Interfaces.Repositories
{
    public interface IDeliveryRequestRepository
    {
        Task<DeliveryRequest> CreateAsync(DeliveryRequest deliveryRequest);
        Task<DeliveryRequest?> GetByIdAsync(int id);
        Task<DeliveryRequest> UpdateAsync(DeliveryRequest deliveryRequest);
        Task<IEnumerable<DeliveryRequest>> GetBySenderIdAsync(string senderId);
        Task<IEnumerable<DeliveryRequest>> GetByTripIdAsync(int tripId);
        Task<IEnumerable<DeliveryRequest>> GetByStatusAsync(DeliveryStatus status);
        Task<IEnumerable<DeliveryRequest>> GetPendingDeliveriesAsync();
        Task<IEnumerable<DeliveryRequest>> GetDeliveriesByStatusAndTripsAsync(DeliveryStatus status, IEnumerable<int> tripIds);
        Task<IEnumerable<DeliveryRequest>> GetPendingRequestsAsync();
        Task<IEnumerable<DeliveryRequest>> GetUserRequestsAsync(string userId);
        Task<IEnumerable<DeliveryRequest>> GetDriverDeliveriesAsync(string driverId);
    }
} 