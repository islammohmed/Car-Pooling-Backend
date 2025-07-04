using CarPooling.Application.DTOs;
using CarPooling.Domain.Entities;

namespace CarPooling.Application.Interfaces.Repositories
{
    public interface IDeliveryRequestRepository
    {
        Task<DeliveryRequest> CreateAsync(DeliveryRequest request);
        Task<DeliveryRequest?> GetByIdAsync(int id);
        Task<List<DeliveryRequest>> GetPendingRequestsAsync();
        Task<List<DeliveryRequest>> GetUserRequestsAsync(string userId);
        Task<List<DeliveryRequest>> GetDriverDeliveriesAsync(string driverId);
        Task<DeliveryRequest> UpdateAsync(DeliveryRequest request);
        Task<bool> DeleteAsync(int id);
    }
} 