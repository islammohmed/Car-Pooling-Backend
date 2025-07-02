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

        public async Task<ApiResponse<bool>> VerifyPassengerNationalId(string userId, PassengerVerificationDto verificationDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                if (verificationDto.NationalIdImage == null)
                {
                    return ApiResponse<bool>.ErrorResponse("National ID image is required for passenger verification");
                }

                // Upload national ID image
                string nationalIdImageUrl;
                using (var stream = verificationDto.NationalIdImage.OpenReadStream())
                {
                    nationalIdImageUrl = await _fileStorage.UploadFileAsync(
                        stream,
                        $"national_id_{userId}_{Path.GetFileName(verificationDto.NationalIdImage.FileName)}"
                    );
                }

                // Check if there's already an active verification for this national ID
                var hasActiveVerification = await _userRepository.HasActiveNationalIdVerificationAsync(userId, nationalIdImageUrl);
                if (hasActiveVerification)
                {
                    // Delete the uploaded image since we can't use it
                    await _fileStorage.DeleteFileAsync(nationalIdImageUrl);
                    return ApiResponse<bool>.ErrorResponse("There is already an active verification request for this national ID");
                }

                // Delete old national ID image if exists
                if (!string.IsNullOrEmpty(user.NationalIdImage))
                {
                    await _fileStorage.DeleteFileAsync(user.NationalIdImage);
                }

                // Update user
                user.NationalIdImage = nationalIdImageUrl;
                user.UserRole = UserRole.Passenger;

                // Create document verification
                var documentVerification = new DocumentVerification
                {
                    UserId = userId,
                    DocumentImage = nationalIdImageUrl,
                    Status = VerificationStatus.Pending,
                    SubmissionDate = DateTime.UtcNow,
                    DocumentType = "NationalId"
                };

                await _userRepository.AddDocumentVerificationAsync(documentVerification);
                await _userRepository.UpdateAsync(user);

                return ApiResponse<bool>.SuccessResponse(true, "National ID submitted successfully. Waiting for admin verification.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error submitting National ID: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UpdateDocument(string userId, string documentType, IFormFile documentFile)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                if (documentFile == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Document file is required");
                }

                // Validate document type based on user role
                if (user.UserRole == UserRole.Passenger && documentType != "NationalId")
                {
                    return ApiResponse<bool>.ErrorResponse("Passengers can only update National ID");
                }

                if (user.UserRole == UserRole.Driver && 
                    documentType != "NationalId" && 
                    documentType != "DrivingLicense" && 
                    documentType != "CarLicense")
                {
                    return ApiResponse<bool>.ErrorResponse("Invalid document type for driver");
                }

                // Upload new document
                string documentImageUrl;
                using (var stream = documentFile.OpenReadStream())
                {
                    documentImageUrl = await _fileStorage.UploadFileAsync(
                        stream,
                        $"{documentType.ToLower()}_{userId}_{Path.GetFileName(documentFile.FileName)}"
                    );
                }

                // Delete old document image based on type
                string? oldImageUrl = null;
                switch (documentType)
                {
                    case "NationalId":
                        oldImageUrl = user.NationalIdImage;
                        user.NationalIdImage = documentImageUrl;
                        break;
                    case "DrivingLicense":
                        if (user.UserRole != UserRole.Driver)
                        {
                            await _fileStorage.DeleteFileAsync(documentImageUrl);
                            return ApiResponse<bool>.ErrorResponse("Only drivers can update driving license");
                        }
                        oldImageUrl = user.DrivingLicenseImage;
                        user.DrivingLicenseImage = documentImageUrl;
                        break;
                    case "CarLicense":
                        if (user.UserRole != UserRole.Driver)
                        {
                            await _fileStorage.DeleteFileAsync(documentImageUrl);
                            return ApiResponse<bool>.ErrorResponse("Only drivers can update car license");
                        }
                        var car = await _userRepository.GetUserCarAsync(userId);
                        if (car == null)
                        {
                            await _fileStorage.DeleteFileAsync(documentImageUrl);
                            return ApiResponse<bool>.ErrorResponse("No car found for this driver");
                        }
                        oldImageUrl = car.CarLicenseImage;
                        car.CarLicenseImage = documentImageUrl;
                        await _userRepository.UpdateCarAsync(car);
                        break;
                    default:
                        await _fileStorage.DeleteFileAsync(documentImageUrl);
                        return ApiResponse<bool>.ErrorResponse("Invalid document type");
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await _fileStorage.DeleteFileAsync(oldImageUrl);
                }

                // Create new document verification
                var documentVerification = new DocumentVerification
                {
                    UserId = userId,
                    DocumentImage = documentImageUrl,
                    Status = VerificationStatus.Pending,
                    SubmissionDate = DateTime.UtcNow,
                    DocumentType = documentType
                };

                await _userRepository.AddDocumentVerificationAsync(documentVerification);
                await _userRepository.UpdateAsync(user);

                return ApiResponse<bool>.SuccessResponse(true, $"{documentType} updated successfully. Waiting for admin verification.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error updating document: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> RegisterAsDriver(string userId, DriverRegistrationDto driverDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                // Validate required documents
                if (driverDto.NationalIdImage == null || driverDto.DrivingLicenseImage == null || driverDto.CarLicenseImage == null)
                {
                    return ApiResponse<bool>.ErrorResponse("All documents (National ID, Driving License, and Car License) are required");
                }

                // Upload National ID
                string nationalIdImageUrl;
                using (var stream = driverDto.NationalIdImage.OpenReadStream())
                {
                    nationalIdImageUrl = await _fileStorage.UploadFileAsync(
                        stream,
                        $"national_id_{userId}_{Path.GetFileName(driverDto.NationalIdImage.FileName)}"
                    );
                }

                // Upload Driving License
                string drivingLicenseImageUrl;
                using (var stream = driverDto.DrivingLicenseImage.OpenReadStream())
                {
                    drivingLicenseImageUrl = await _fileStorage.UploadFileAsync(
                        stream,
                        $"driving_license_{userId}_{Path.GetFileName(driverDto.DrivingLicenseImage.FileName)}"
                    );
                }

                // Upload Car License
                string carLicenseImageUrl;
                using (var stream = driverDto.CarLicenseImage.OpenReadStream())
                {
                    carLicenseImageUrl = await _fileStorage.UploadFileAsync(
                        stream,
                        $"car_license_{userId}_{Path.GetFileName(driverDto.CarLicenseImage.FileName)}"
                    );
                }

                // Create car entity
                var car = new Car
                {
                    Model = driverDto.Model,
                    Color = driverDto.Color,
                    PlateNumber = driverDto.PlateNumber,
                    CarLicenseImage = carLicenseImageUrl,
                    DriverId = userId
                };

                // Update user
                user.NationalIdImage = nationalIdImageUrl;
                user.DrivingLicenseImage = drivingLicenseImageUrl;
                user.UserRole = UserRole.Driver;

                // Create document verifications
                var documentVerifications = new List<DocumentVerification>
                {
                    new DocumentVerification
                    {
                        UserId = userId,
                        DocumentImage = nationalIdImageUrl,
                        Status = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow,
                        DocumentType = "NationalId"
                    },
                    new DocumentVerification
                    {
                        UserId = userId,
                        DocumentImage = drivingLicenseImageUrl,
                        Status = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow,
                        DocumentType = "DrivingLicense"
                    },
                    new DocumentVerification
                    {
                        UserId = userId,
                        DocumentImage = carLicenseImageUrl,
                        Status = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow,
                        DocumentType = "CarLicense"
                    }
                };

                // Save all changes
                foreach (var doc in documentVerifications)
                {
                    await _userRepository.AddDocumentVerificationAsync(doc);
                }

                await _userRepository.AddCarAsync(car);
                await _userRepository.UpdateAsync(user);

                return ApiResponse<bool>.SuccessResponse(true, "Driver registration submitted successfully. Waiting for admin verification of documents.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error registering as driver: {ex.Message}");
            }
        }
    }
}