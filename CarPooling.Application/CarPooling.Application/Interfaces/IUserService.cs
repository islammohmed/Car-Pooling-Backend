using CarPooling.Application.DTOs;
using Microsoft.AspNetCore.Http;
using CarPooling.Domain.DTOs;
using CarPooling.Application.Common.Results;

namespace CarPooling.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<bool>> VerifyPassengerNationalId(string userId, PassengerVerificationDto verificationDto);
        Task<Result<bool>> RegisterAsDriver(string userId, DriverRegistrationDto driverDto);
        Task<Result<bool>> UpdateDocument(string userId, string documentType, IFormFile documentFile);
        Task<Result<List<DocumentVerificationDto>>> GetUserDocumentVerifications(string userId);
        Task<Result<List<UserDto>>> GetAllUsersAsync();
        Task<Result<UserDto>> GetUserByIdAsync(string userId);
        Task<Result<bool>> DeleteUserAsync(string userId, string adminId);
        Task<Result<bool>> BlockUserAsync(string userId, string adminId);
        Task<Result<bool>> UnblockUserAsync(string userId, string adminId);
    }
}