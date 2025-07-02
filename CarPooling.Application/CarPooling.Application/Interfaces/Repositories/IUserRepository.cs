using System.Threading.Tasks;
using CarPooling.Domain.Entities;

namespace CarPooling.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(string userId);
        Task UpdateAsync(User user);
        Task<bool> HasActiveNationalIdVerificationAsync(string userId, string nationalIdImage);
        Task AddDocumentVerificationAsync(DocumentVerification verification);
    }
} 