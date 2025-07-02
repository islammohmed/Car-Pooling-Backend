using CarPooling.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace CarPooling.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<bool>> VerifyPassengerNationalId(string userId, PassengerVerificationDto verificationDto);
        Task<ApiResponse<bool>> RegisterAsDriver(string userId, DriverRegistrationDto driverDto);
        Task<ApiResponse<bool>> UpdateDocument(string userId, string documentType, IFormFile documentFile);
    }
}