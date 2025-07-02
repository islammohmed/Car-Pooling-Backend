using CarPooling.Application.Interfaces.Repositories;

namespace CarPooling.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ITripRepository Trips { get; }
        IUserRepository Users { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 