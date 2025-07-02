using CarPooling.Application.DTOs;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CarPooling.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorage;

        public UserService(IUserRepository userRepository, IFileStorageService fileStorage)
        {
            _userRepository = userRepository;
            _fileStorage = fileStorage;
        }

        public async Task<ApiResponse<bool>> CompleteUserProfile(string userId, EditUserProfileDto profileDto, IFormFile? nationalIdFile = null)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                // Upload national ID image if provided
                string? nationalIdImageUrl = null;
                if (nationalIdFile != null)
                {
                    using var stream = nationalIdFile.OpenReadStream();
                    nationalIdImageUrl = await _fileStorage.UploadFileAsync(
                        stream,
                        $"national_id_{userId}_{Path.GetFileName(nationalIdFile.FileName)}"
                    );
                    
                    // Delete old national ID image if exists
                    if (!string.IsNullOrEmpty(user.NationalIdImage))
                    {
                        await _fileStorage.DeleteFileAsync(user.NationalIdImage);
                    }
                    
                    user.NationalIdImage = nationalIdImageUrl;
                }

                // Check if there's already an active verification for this national ID
                if (!string.IsNullOrEmpty(nationalIdImageUrl))
                {
                    var hasActiveVerification = await _userRepository.HasActiveNationalIdVerificationAsync(userId, nationalIdImageUrl);
                    if (hasActiveVerification)
                    {
                        // Delete the uploaded image since we can't use it
                        await _fileStorage.DeleteFileAsync(nationalIdImageUrl);
                        return ApiResponse<bool>.ErrorResponse("There is already an active verification request for this national ID");
                    }
                }

                // Update user profile
                user.FirstName = profileDto.FirstName ?? user.FirstName;
                user.LastName = profileDto.LastName ?? user.LastName;
                user.PhoneNumber = profileDto.PhoneNumber ?? user.PhoneNumber;
                user.Gender = profileDto.Gender ?? user.Gender;
                user.SSN = profileDto.SSN ?? user.SSN;

                // Create document verification if national ID image was uploaded
                if (!string.IsNullOrEmpty(nationalIdImageUrl))
                {
                    var documentVerification = new DocumentVerification
                    {
                        UserId = userId,
                        DocumentImage = nationalIdImageUrl,
                        Status = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow,
                        DocumentType = "NationalId"
                    };

                    await _userRepository.AddDocumentVerificationAsync(documentVerification);
                }

                await _userRepository.UpdateAsync(user);

                return ApiResponse<bool>.SuccessResponse(true, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error updating profile: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UploadDrivingLicense(string userId, DrivingLicenseUploadDto uploadDto)
        {
            try
            {
                if (uploadDto.LicenseFile == null)
                {
                    return ApiResponse<bool>.ErrorResponse("No license file provided");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                using var stream = uploadDto.LicenseFile.OpenReadStream();
                var licenseImageUrl = await _fileStorage.UploadFileAsync(
                    stream,
                    $"driving_license_{userId}_{Path.GetFileName(uploadDto.LicenseFile.FileName)}"
                );

                // Delete old license image if exists
                if (!string.IsNullOrEmpty(user.DrivingLicenseImage))
                {
                    await _fileStorage.DeleteFileAsync(user.DrivingLicenseImage);
                }

                user.DrivingLicenseImage = licenseImageUrl;

                // Create document verification for driving license
                var documentVerification = new DocumentVerification
                {
                    UserId = userId,
                    DocumentImage = licenseImageUrl,
                    Status = VerificationStatus.Pending,
                    SubmissionDate = DateTime.UtcNow,
                    DocumentType = "DrivingLicense"
                };

                await _userRepository.AddDocumentVerificationAsync(documentVerification);
                await _userRepository.UpdateAsync(user);

                return ApiResponse<bool>.SuccessResponse(true, "Driving license uploaded successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error uploading driving license: {ex.Message}");
            }
        }
    }
}