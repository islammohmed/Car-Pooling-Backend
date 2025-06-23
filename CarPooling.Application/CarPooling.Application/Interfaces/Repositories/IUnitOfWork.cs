using CarPooling.Application.Repositories;


namespace CarPooling.Application.Interfaces
{
    public class IUnitOfWork
    {
        ITripRepository Trips { get; }
      

      //  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
