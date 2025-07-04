using CarPooling.Application.Interfaces;
using CarPooling.Application.Interfaces.Repositories;

namespace CarPooling.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ITripRepository _tripRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDeliveryRequestRepository _deliveryRequestRepository;
        private readonly IFeedbackRepository _feedbackRepository;

        public UnitOfWork(
            AppDbContext context, 
            ITripRepository tripRepository, 
            IUserRepository userRepository,
            IDeliveryRequestRepository deliveryRequestRepository,
            IFeedbackRepository feedbackRepository)
        {
            _context = context;
            _tripRepository = tripRepository;
            _userRepository = userRepository;
            _deliveryRequestRepository = deliveryRequestRepository;
            _feedbackRepository = feedbackRepository;
        }

        public ITripRepository Trips => _tripRepository;
        public IUserRepository Users => _userRepository;
        public IDeliveryRequestRepository DeliveryRequests => _deliveryRequestRepository;
        public IFeedbackRepository Feedbacks => _feedbackRepository;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 