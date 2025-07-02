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

        public async Task<User> GetByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasActiveNationalIdVerificationAsync(string userId, string nationalIdImage)
        {
            return await _context.DocumentVerifications.AnyAsync(dv =>
                dv.UserId == userId &&
                dv.DocumentType == "national id" &&
                (dv.VerificationStatus == VerificationStatus.Pending || dv.VerificationStatus == VerificationStatus.Approved)
            );
        }

        public async Task AddDocumentVerificationAsync(DocumentVerification verification)
        {
            _context.DocumentVerifications.Add(verification);
            await _context.SaveChangesAsync();
        }
    }
} 