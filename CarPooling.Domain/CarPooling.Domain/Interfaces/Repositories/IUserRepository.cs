using CarPooling.Domain.Entities;

namespace CarPooling.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UpdateUserAsync(User user);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<DocumentVerification>> GetUserDocumentVerificationsAsync(string userId);
        Task<bool> AddDocumentVerificationAsync(DocumentVerification documentVerification);
        Task<bool> UpdateDocumentVerificationAsync(DocumentVerification documentVerification);
        Task<DocumentVerification?> GetPendingNationalIdVerificationAsync(string userId);
        Task<IEnumerable<DocumentVerification>> GetPendingDriverVerificationsAsync(string userId);
        Task<DocumentVerification?> GetDocumentVerificationByIdAsync(int id);
        Task<Car?> GetUserCarAsync(string userId);
        Task<bool> UpdateCarAsync(Car car);
        Task<IEnumerable<DocumentVerification>> GetPendingDocumentVerificationsAsync();
        Task<bool> AddCarAsync(Car car);
        Task<bool> DeleteUserAsync(string userId);
    }
} 