using CarPooling.Domain.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
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
            return await _context.DeliveryRequests
                .Include(d => d.Sender)
                .Include(d => d.Trip)
                    .ThenInclude(t => t.Driver)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<DeliveryRequest>> GetPendingRequestsAsync()
        {
            return await _context.DeliveryRequests
                .Include(d => d.Sender)
                .Where(d => d.Status == "Pending")
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryRequest>> GetUserRequestsAsync(string userId)
        {
            return await _context.DeliveryRequests
                .Include(d => d.Sender)
                .Include(d => d.Trip)
                    .ThenInclude(t => t.Driver)
                .Where(d => d.SenderId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryRequest>> GetDriverDeliveriesAsync(string driverId)
        {
            return await _context.DeliveryRequests
                .Include(d => d.Sender)
                .Include(d => d.Trip)
                .Where(d => d.Trip != null && d.Trip.DriverId == driverId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryRequest>> GetDeliveriesByStatusAndTripsAsync(string status, IEnumerable<int> tripIds)
        {
            return await _context.DeliveryRequests
                .Include(d => d.Sender)
                .Include(d => d.Trip)
                    .ThenInclude(t => t.Driver)
                .Where(d => d.Status == status && d.TripId.HasValue && tripIds.Contains(d.TripId.Value))
                .ToListAsync();
        }

        public async Task<DeliveryRequest> UpdateAsync(DeliveryRequest request)
        {
            _context.DeliveryRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<IEnumerable<DeliveryRequest>> GetBySenderIdAsync(string senderId)
        {
            return await _context.DeliveryRequests
                .Include(d => d.Sender)
                .Include(d => d.Trip)
                .Where(d => d.SenderId == senderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryRequest>> GetByTripIdAsync(int tripId)
        {
            return await _context.DeliveryRequests
                .Include(d => d.Sender)
                .Include(d => d.Trip)
                .Where(d => d.TripId == tripId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryRequest>> GetByStatusAsync(DeliveryStatus status)
        {
            return await _context.DeliveryRequests
                .Include(d => d.Sender)
                .Include(d => d.Trip)
                .Where(d => d.Status == status.ToString())
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryRequest>> GetPendingDeliveriesAsync()
        {
            return await _context.DeliveryRequests
                .Include(d => d.Sender)
                .Include(d => d.Trip)
                .Where(d => d.Status == DeliveryStatus.Pending.ToString())
                .ToListAsync();
        }
    }
} 