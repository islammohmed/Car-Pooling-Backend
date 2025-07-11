using CarPooling.Application.DTOs;
using Microsoft.AspNetCore.Http;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<bool>> VerifyPassengerNationalId(string userId, PassengerVerificationDto verificationDto);
        Task<ApiResponse<bool>> RegisterAsDriver(string userId, DriverRegistrationDto driverDto);
        Task<ApiResponse<bool>> UpdateDocument(string userId, string documentType, IFormFile documentFile);
        Task<ApiResponse<List<DocumentVerificationDto>>> GetUserDocumentVerifications(string userId);
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync();
        Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId);
        Task<ApiResponse<bool>> DeleteUserAsync(string userId, string adminId);
        Task<ApiResponse<bool>> BlockUserAsync(string userId, string adminId);
        Task<ApiResponse<bool>> UnblockUserAsync(string userId, string adminId);
    }
}