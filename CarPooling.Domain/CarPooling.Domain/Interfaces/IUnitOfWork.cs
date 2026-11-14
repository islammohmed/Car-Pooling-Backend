using CarPooling.Domain.Interfaces.Repositories;

namespace CarPooling.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        ITripRepository Trips { get; }
        IUserRepository Users { get; }
        IDeliveryRequestRepository DeliveryRequests { get; }
        IFeedbackRepository Feedbacks { get; }
        
        Task<int> SaveChangesAsync();
    }
} 