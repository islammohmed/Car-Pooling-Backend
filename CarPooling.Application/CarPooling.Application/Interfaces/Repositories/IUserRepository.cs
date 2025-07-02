using System.Threading.Tasks;
using CarPooling.Domain.Entities;

namespace CarPooling.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(User user);
        Task<bool> HasActiveNationalIdVerificationAsync(string userId, string nationalIdImageUrl);
        Task<bool> AddDocumentVerificationAsync(DocumentVerification documentVerification);
        Task<bool> AddCarAsync(Car car);
        Task<Car?> GetUserCarAsync(string userId);
        Task<bool> UpdateCarAsync(Car car);
    }
} 