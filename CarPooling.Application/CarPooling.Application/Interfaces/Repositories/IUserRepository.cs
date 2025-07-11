using System.Threading.Tasks;
using CarPooling.Domain.Entities;

namespace CarPooling.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllUsersAsync();
        Task<List<DocumentVerification>> GetPendingDocumentVerificationsAsync();
        Task<DocumentVerification?> GetDocumentVerificationByIdAsync(int id);
        Task<List<DocumentVerification>> GetUserDocumentVerificationsAsync(string userId);
        Task<DocumentVerification?> GetPendingNationalIdVerificationAsync(string userId);
        Task<List<DocumentVerification>> GetPendingDriverVerificationsAsync(string userId);
        Task AddDocumentVerificationAsync(DocumentVerification verification);
        Task UpdateDocumentVerificationAsync(DocumentVerification verification);
        Task<Car?> GetUserCarAsync(string userId);
        Task UpdateCarAsync(Car car);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> AddCarAsync(Car car);
        Task<bool> DeleteUserAsync(string userId);
    }
} 