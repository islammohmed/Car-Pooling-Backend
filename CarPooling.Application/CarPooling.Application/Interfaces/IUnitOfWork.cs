using CarPooling.Application.Interfaces.Repositories;

namespace CarPooling.Application.Interfaces
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