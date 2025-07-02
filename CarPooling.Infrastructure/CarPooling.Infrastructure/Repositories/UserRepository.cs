using System.Threading.Tasks;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarPooling.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> HasActiveNationalIdVerificationAsync(string userId, string nationalIdImageUrl)
        {
            return await _context.DocumentVerifications
                .AnyAsync(d => d.UserId == userId &&
                              d.DocumentType == "NationalId" &&
                              d.Status == VerificationStatus.Pending);
        }

        public async Task<bool> AddDocumentVerificationAsync(DocumentVerification documentVerification)
        {
            await _context.DocumentVerifications.AddAsync(documentVerification);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> AddCarAsync(Car car)
        {
            await _context.Cars.AddAsync(car);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<Car?> GetUserCarAsync(string userId)
        {
            return await _context.Cars
                .FirstOrDefaultAsync(c => c.DriverId == userId);
        }

        public async Task<bool> UpdateCarAsync(Car car)
        {
            _context.Cars.Update(car);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
} 