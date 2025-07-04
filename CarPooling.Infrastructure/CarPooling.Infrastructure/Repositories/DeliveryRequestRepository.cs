using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using CarPooling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarPooling.Infrastructure.Repositories
{
    public class DeliveryRequestRepository : IDeliveryRequestRepository
    {
        private readonly AppDbContext _context;

        public DeliveryRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DeliveryRequest> CreateAsync(DeliveryRequest request)
        {
            await _context.DeliveryRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<DeliveryRequest?> GetByIdAsync(int id)
        {
            return await _context.DeliveryRequests.Include(d => d.Sender).FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<DeliveryRequest>> GetPendingRequestsAsync()
        {
            return await _context.DeliveryRequests.Where(d => d.Status == "Pending").ToListAsync();
        }

        public async Task<List<DeliveryRequest>> GetUserRequestsAsync(string userId)
        {
            return await _context.DeliveryRequests.Where(d => d.SenderId == userId).ToListAsync();
        }

        public async Task<List<DeliveryRequest>> GetDriverDeliveriesAsync(string driverId)
        {
            return await _context.DeliveryRequests.Where(d => d.Trip != null && d.Trip.DriverId == driverId).ToListAsync();
        }

        public async Task<DeliveryRequest> UpdateAsync(DeliveryRequest request)
        {
            _context.DeliveryRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.DeliveryRequests.FindAsync(id);
            if (entity == null) return false;
            _context.DeliveryRequests.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 