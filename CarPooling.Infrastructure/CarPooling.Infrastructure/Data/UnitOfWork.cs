using CarPooling.Application.Interfaces;
using CarPooling.Application.Interfaces.Repositories;

namespace CarPooling.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ITripRepository _tripRepository;
        private readonly IUserRepository _userRepository;

        public UnitOfWork(AppDbContext context, ITripRepository tripRepository, IUserRepository userRepository)
        {
            _context = context;
            _tripRepository = tripRepository;
            _userRepository = userRepository;
        }

        public ITripRepository Trips => _tripRepository;
        public IUserRepository Users => _userRepository;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 