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

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserAsync(User user)
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
                              d.VerificationStatus == VerificationStatus.Pending);
        }

        public async Task AddDocumentVerificationAsync(DocumentVerification verification)
        {
            await _context.DocumentVerifications.AddAsync(verification);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddCarAsync(Car car)
        {
            await _context.Cars.AddAsync(car);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<Car?> GetUserCarAsync(string userId)
        {
            return await _context.Cars.FirstOrDefaultAsync(c => c.DriverId == userId);
        }

        public async Task UpdateCarAsync(Car car)
        {
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DocumentVerification>> GetPendingDocumentVerificationsAsync()
        {
            return await _context.DocumentVerifications
                .Include(d => d.User)
                .Where(d => d.VerificationStatus == VerificationStatus.Pending)
                .OrderBy(d => d.SubmissionDate)
                .ToListAsync();
        }

        public async Task<DocumentVerification?> GetDocumentVerificationByIdAsync(int id)
        {
            return await _context.DocumentVerifications
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task UpdateDocumentVerificationAsync(DocumentVerification verification)
        {
            _context.DocumentVerifications.Update(verification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DocumentVerification>> GetUserDocumentVerificationsAsync(string userId)
        {
            return await _context.DocumentVerifications
                .Include(d => d.User)
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.SubmissionDate)
                .ToListAsync();
        }

        public async Task<DocumentVerification?> GetPendingNationalIdVerificationAsync(string userId)
        {
            return await _context.DocumentVerifications
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId && 
                                        d.DocumentType == "NationalId" && 
                                        d.VerificationStatus == VerificationStatus.Pending);
        }

        public async Task<List<DocumentVerification>> GetPendingDriverVerificationsAsync(string userId)
        {
            return await _context.DocumentVerifications
                .Include(d => d.User)
                .Where(d => d.UserId == userId && 
                           d.VerificationStatus == VerificationStatus.Pending &&
                           (d.DocumentType == "NationalId" || 
                            d.DocumentType == "DrivingLicense" || 
                            d.DocumentType == "CarLicense"))
                .OrderBy(d => d.SubmissionDate)
                .ToListAsync();
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                // First, find the user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Delete related document verifications
                var documentVerifications = await _context.DocumentVerifications
                    .Where(d => d.UserId == userId)
                    .ToListAsync();
                
                if (documentVerifications.Any())
                {
                    _context.DocumentVerifications.RemoveRange(documentVerifications);
                }

                // Delete related cars
                var cars = await _context.Cars
                    .Where(c => c.DriverId == userId)
                    .ToListAsync();
                
                if (cars.Any())
                {
                    _context.Cars.RemoveRange(cars);
                }

                // Delete the user
                _context.Users.Remove(user);
                var result = await _context.SaveChangesAsync();
                
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
} 