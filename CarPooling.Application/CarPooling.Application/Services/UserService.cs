using CarPooling.Application.DTOs;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.Exceptions;
using CarPooling.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IO;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;

        public UserService(IUserRepository userRepository, IFileStorageService fileStorageService)
        {
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
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

                if (user.UserRole != UserRole.Passenger)
                {
                    return ApiResponse<bool>.ErrorResponse("Only passengers can submit national ID verification");
                }

                // Check if there's a pending verification
                var existingVerification = await _userRepository.GetPendingNationalIdVerificationAsync(userId);
                if (existingVerification != null)
                {
                    return ApiResponse<bool>.ErrorResponse("You already have a pending national ID verification request");
                }

                // Upload the document image
                string imageUrl;
                using (var stream = verificationDto.NationalIdImage.OpenReadStream())
                {
                    imageUrl = await _fileStorageService.UploadFileAsync(stream, $"national_id_{userId}_{verificationDto.NationalIdImage.FileName}");
                }

                // Create new document verification
                var documentVerification = new DocumentVerification
                {
                    UserId = userId,
                    DocumentType = "NationalId",
                    DocumentImage = imageUrl,
                    VerificationStatus = VerificationStatus.Pending,
                    SubmissionDate = DateTime.UtcNow
                };

                await _userRepository.AddDocumentVerificationAsync(documentVerification);

                return ApiResponse<bool>.SuccessResponse(true, "National ID verification submitted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error submitting national ID verification: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> VerifyDriverDocuments(string userId, DriverVerificationDto verificationDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                if (user.UserRole != UserRole.Driver)
                {
                    return ApiResponse<bool>.ErrorResponse("Only drivers can submit these documents");
                }

                // Check if there are pending verifications
                var pendingVerifications = await _userRepository.GetPendingDriverVerificationsAsync(userId);
                if (pendingVerifications.Any())
                {
                    return ApiResponse<bool>.ErrorResponse("You already have pending document verification requests");
                }

                var documents = new List<DocumentVerification>();

                // Process National ID
                if (verificationDto.NationalIdImage != null)
                {
                    string nationalIdUrl;
                    using (var stream = verificationDto.NationalIdImage.OpenReadStream())
                    {
                        nationalIdUrl = await _fileStorageService.UploadFileAsync(stream, $"national_id_{userId}_{verificationDto.NationalIdImage.FileName}");
                    }
                    documents.Add(new DocumentVerification
                    {
                        UserId = userId,
                        DocumentType = "NationalId",
                        DocumentImage = nationalIdUrl,
                        VerificationStatus = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow
                    });
                }

                // Process Driving License
                if (verificationDto.DrivingLicenseImage != null)
                {
                    string drivingLicenseUrl;
                    using (var stream = verificationDto.DrivingLicenseImage.OpenReadStream())
                    {
                        drivingLicenseUrl = await _fileStorageService.UploadFileAsync(stream, $"driving_license_{userId}_{verificationDto.DrivingLicenseImage.FileName}");
                    }
                    documents.Add(new DocumentVerification
                    {
                        UserId = userId,
                        DocumentType = "DrivingLicense",
                        DocumentImage = drivingLicenseUrl,
                        VerificationStatus = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow
                    });
                }

                // Process Car License
                if (verificationDto.CarLicenseImage != null)
                {
                    string carLicenseUrl;
                    using (var stream = verificationDto.CarLicenseImage.OpenReadStream())
                    {
                        carLicenseUrl = await _fileStorageService.UploadFileAsync(stream, $"car_license_{userId}_{verificationDto.CarLicenseImage.FileName}");
                    }
                    documents.Add(new DocumentVerification
                    {
                        UserId = userId,
                        DocumentType = "CarLicense",
                        DocumentImage = carLicenseUrl,
                        VerificationStatus = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow
                    });
                }

                if (!documents.Any())
                {
                    return ApiResponse<bool>.ErrorResponse("No documents provided for verification");
                }

                foreach (var doc in documents)
                {
                    await _userRepository.AddDocumentVerificationAsync(doc);
                }

                return ApiResponse<bool>.SuccessResponse(true, "Documents submitted for verification successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error submitting documents for verification: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<DocumentVerificationDto>>> GetUserDocumentVerifications(string userId)
        {
            try
            {
                var verifications = await _userRepository.GetUserDocumentVerificationsAsync(userId);
                var dtos = verifications.Select(v => new DocumentVerificationDto
                {
                    Id = v.Id,
                    UserId = v.UserId,
                    DocumentType = v.DocumentType,
                    DocumentImage = v.DocumentImage,
                    Status = v.VerificationStatus,
                    SubmissionDate = v.SubmissionDate
                }).ToList();

                return ApiResponse<List<DocumentVerificationDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DocumentVerificationDto>>.ErrorResponse($"Error getting user verifications: {ex.Message}");
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
                    documentImageUrl = await _fileStorageService.UploadFileAsync(
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
                            await _fileStorageService.DeleteFileAsync(documentImageUrl);
                            return ApiResponse<bool>.ErrorResponse("Only drivers can update driving license");
                        }
                        oldImageUrl = user.DrivingLicenseImage;
                        user.DrivingLicenseImage = documentImageUrl;
                        break;
                    case "CarLicense":
                        if (user.UserRole != UserRole.Driver)
                        {
                            await _fileStorageService.DeleteFileAsync(documentImageUrl);
                            return ApiResponse<bool>.ErrorResponse("Only drivers can update car license");
                        }
                        var car = await _userRepository.GetUserCarAsync(userId);
                        if (car == null)
                        {
                            await _fileStorageService.DeleteFileAsync(documentImageUrl);
                            return ApiResponse<bool>.ErrorResponse("No car found for this driver");
                        }
                        oldImageUrl = car.CarLicenseImage;
                        car.CarLicenseImage = documentImageUrl;
                        await _userRepository.UpdateCarAsync(car);
                        break;
                    default:
                        await _fileStorageService.DeleteFileAsync(documentImageUrl);
                        return ApiResponse<bool>.ErrorResponse("Invalid document type");
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(oldImageUrl);
                }

                // Create new document verification
                var documentVerification = new DocumentVerification
                {
                    UserId = userId,
                    DocumentImage = documentImageUrl,
                    VerificationStatus = VerificationStatus.Pending,
                    SubmissionDate = DateTime.UtcNow,
                    DocumentType = documentType
                };

                await _userRepository.AddDocumentVerificationAsync(documentVerification);
                await _userRepository.UpdateUserAsync(user);

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

                // Check if user is already a driver
                if (user.UserRole == UserRole.Driver)
                {
                    return ApiResponse<bool>.ErrorResponse("User is already registered as a driver");
                }

                // Check for pending driver verifications
                var pendingVerifications = await _userRepository.GetPendingDriverVerificationsAsync(user.Id);
                if (pendingVerifications.Any())
                {
                    return ApiResponse<bool>.ErrorResponse("You already have pending driver registration documents. Please wait for admin approval.");
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
                    nationalIdImageUrl = await _fileStorageService.UploadFileAsync(
                        stream,
                        $"national_id_{userId}_{Path.GetFileName(driverDto.NationalIdImage.FileName)}"
                    );
                }

                // Upload Driving License
                string drivingLicenseImageUrl;
                using (var stream = driverDto.DrivingLicenseImage.OpenReadStream())
                {
                    drivingLicenseImageUrl = await _fileStorageService.UploadFileAsync(
                        stream,
                        $"driving_license_{userId}_{Path.GetFileName(driverDto.DrivingLicenseImage.FileName)}"
                    );
                }

                // Upload Car License
                string carLicenseImageUrl;
                using (var stream = driverDto.CarLicenseImage.OpenReadStream())
                {
                    carLicenseImageUrl = await _fileStorageService.UploadFileAsync(
                        stream,
                        $"car_license_{userId}_{Path.GetFileName(driverDto.CarLicenseImage.FileName)}"
                    );
                }

                // Create document verifications for admin approval
                var documents = new List<DocumentVerification>
                {
                    new DocumentVerification
                    {
                        UserId = userId,
                        DocumentType = "NationalId",
                        DocumentImage = nationalIdImageUrl,
                        VerificationStatus = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow
                    },
                    new DocumentVerification
                    {
                        UserId = userId,
                        DocumentType = "DrivingLicense",
                        DocumentImage = drivingLicenseImageUrl,
                        VerificationStatus = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow
                    },
                    new DocumentVerification
                    {
                        UserId = userId,
                        DocumentType = "CarLicense",
                        DocumentImage = carLicenseImageUrl,
                        VerificationStatus = VerificationStatus.Pending,
                        SubmissionDate = DateTime.UtcNow
                    }
                };

                // Add document verifications
                foreach (var doc in documents)
                {
                    await _userRepository.AddDocumentVerificationAsync(doc);
                }

                // Create car (but set as unverified)
                var car = new Car
                {
                    DriverId = userId,
                    CarLicenseImage = carLicenseImageUrl,
                    IsVerified = false,
                    Model = driverDto.Model,
                    Color = driverDto.Color,
                    PlateNumber = driverDto.PlateNumber
                };

                await _userRepository.AddCarAsync(car);

                return ApiResponse<bool>.SuccessResponse(true, "Driver registration documents submitted successfully. Waiting for admin verification.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error submitting driver documents: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                var userDtos = users.Select(MapUserToDto).ToList();
                return ApiResponse<List<UserDto>>.SuccessResponse(userDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserDto>>.ErrorResponse($"Error retrieving users: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found");
                }

                var userDto = MapUserToDto(user);
                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(string userId, string adminId)
        {
            try
            {
                // Verify admin
                var admin = await _userRepository.GetByIdAsync(adminId);
                if (admin == null || admin.UserRole != UserRole.Admin)
                {
                    return ApiResponse<bool>.ErrorResponse("Unauthorized: Only admins can delete users");
                }

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                // Don't allow admins to delete other admins
                if (user.UserRole == UserRole.Admin)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete admin users");
                }

                // Delete the user
                var result = await _userRepository.DeleteUserAsync(userId);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("Failed to delete user");
                }

                return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error deleting user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> BlockUserAsync(string userId, string adminId)
        {
            try
            {
                // Verify admin
                var admin = await _userRepository.GetByIdAsync(adminId);
                if (admin == null || admin.UserRole != UserRole.Admin)
                {
                    return ApiResponse<bool>.ErrorResponse("Unauthorized: Only admins can block users");
                }

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                // Don't allow admins to block other admins
                if (user.UserRole == UserRole.Admin)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot block admin users");
                }

                // Check if user is already blocked
                if (user.IsBlocked)
                {
                    return ApiResponse<bool>.ErrorResponse("User is already blocked");
                }

                // Block the user
                user.IsBlocked = true;
                var result = await _userRepository.UpdateUserAsync(user);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("Failed to block user");
                }

                return ApiResponse<bool>.SuccessResponse(true, "User blocked successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error blocking user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UnblockUserAsync(string userId, string adminId)
        {
            try
            {
                // Verify admin
                var admin = await _userRepository.GetByIdAsync(adminId);
                if (admin == null || admin.UserRole != UserRole.Admin)
                {
                    return ApiResponse<bool>.ErrorResponse("Unauthorized: Only admins can unblock users");
                }

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                // Check if user is already unblocked
                if (!user.IsBlocked)
                {
                    return ApiResponse<bool>.ErrorResponse("User is not blocked");
                }

                // Unblock the user
                user.IsBlocked = false;
                var result = await _userRepository.UpdateUserAsync(user);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("Failed to unblock user");
                }

                return ApiResponse<bool>.SuccessResponse(true, "User unblocked successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error unblocking user: {ex.Message}");
            }
        }

        private UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserRole = user.UserRole,
                Gender = user.Gender ?? Gender.Any, // Use Gender.Any as default
                AvgRating = user.AvgRating,
                ProfileImage = user.NationalIdImage,
                HasLoggedIn = user.HasLoggedIn,
                IsVerified = user.IsVerified,
                IsBlocked = user.IsBlocked
            };
        }
    }
}