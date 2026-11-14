using CarPooling.Domain.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using CarPooling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarPooling.Infrastructure.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly AppDbContext _context;

        public FeedbackRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Feedback> CreateAsync(Feedback feedback)
        {
            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await _context.Feedbacks
                .Include(f => f.Sender)
                .Include(f => f.Receiver)
                .Include(f => f.Trip)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Feedback>> GetUserReceivedFeedbacksAsync(string userId)
        {
            return await _context.Feedbacks
                .Include(f => f.Sender)
                .Include(f => f.Trip)
                .Where(f => f.ReceiverId == userId)
                .OrderByDescending(f => f.Trip.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetTripFeedbacksAsync(int tripId)
        {
            return await _context.Feedbacks
                .Include(f => f.Sender)
                .Include(f => f.Receiver)
                .Include(f => f.Trip)
                .Where(f => f.TripId == tripId)
                .ToListAsync();
        }

        public async Task<bool> HasUserFeedbackForTripAsync(string senderId, int tripId)
        {
            return await _context.Feedbacks
                .AnyAsync(f => f.SenderId == senderId && f.TripId == tripId);
        }

        public async Task<double> GetUserAverageRatingAsync(string userId)
        {
            var ratings = await _context.Feedbacks
                .Where(f => f.ReceiverId == userId)
                .Select(f => f.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }

        public async Task<int> GetUserTotalFeedbacksAsync(string userId)
        {
            return await _context.Feedbacks
                .CountAsync(f => f.ReceiverId == userId);
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByReceiverIdAsync(string receiverId)
        {
            return await _context.Feedbacks
                .Include(f => f.Sender)
                .Include(f => f.Trip)
                .Where(f => f.ReceiverId == receiverId)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingForUserAsync(string userId)
        {
            var ratings = await _context.Feedbacks
                .Where(f => f.ReceiverId == userId)
                .Select(f => f.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }
    }
} 