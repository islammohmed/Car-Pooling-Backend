using CarPooling.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace CarPooling.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<bool>> CompleteUserProfile(string userId, EditUserProfileDto profileDto, IFormFile? nationalIdFile = null);
        Task<ApiResponse<bool>> UploadDrivingLicense(string userId, DrivingLicenseUploadDto uploadDto);
    }
}