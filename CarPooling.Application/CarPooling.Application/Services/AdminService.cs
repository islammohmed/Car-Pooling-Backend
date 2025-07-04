using CarPooling.Application.DTOs;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;

        public AdminService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<List<DocumentVerificationDto>>> GetPendingDocumentVerifications()
        {
            try
            {
                var verifications = await _userRepository.GetPendingDocumentVerificationsAsync();
                var dtos = verifications.Select(v => new DocumentVerificationDto
                {
                    Id = v.Id,
                    UserId = v.UserId,
                    UserFullName = $"{v.User.FirstName} {v.User.LastName}",
                    DocumentType = v.DocumentType,
                    DocumentImage = v.DocumentImage,
                    Status = v.VerificationStatus,
                    SubmissionDate = v.SubmissionDate
                }).ToList();

                return ApiResponse<List<DocumentVerificationDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DocumentVerificationDto>>.ErrorResponse($"Error getting pending verifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> HandleDocumentVerification(string adminId, DocumentVerificationActionDto actionDto)
        {
            try
            {
                var verification = await _userRepository.GetDocumentVerificationByIdAsync(actionDto.DocumentId);
                if (verification == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Document verification not found");
                }

                var user = await _userRepository.GetByIdAsync(verification.UserId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                string message = "Document verification updated successfully";

                // Update verification status
                verification.VerificationStatus = actionDto.NewStatus;
                verification.AdminId = adminId;
                verification.VerifiedAt = DateTime.UtcNow;
                verification.RejectionReason = actionDto.RejectionReason;

                // If rejected, we don't need to do anything else
                if (actionDto.NewStatus == VerificationStatus.Rejected)
                {
                    await _userRepository.UpdateDocumentVerificationAsync(verification);
                    return ApiResponse<bool>.SuccessResponse(true, "Document verification rejected");
                }

                // For approved documents, check if all required documents are approved
                if (actionDto.NewStatus == VerificationStatus.Approved)
                {
                    var allDocuments = await _userRepository.GetUserDocumentVerificationsAsync(user.Id);

                    // Check for at least one approved document for each type
                    bool approvedNationalId = allDocuments.Any(d => d.DocumentType == "NationalId" && d.VerificationStatus == VerificationStatus.Approved);
                    bool approvedDrivingLicense = allDocuments.Any(d => d.DocumentType == "DrivingLicense" && d.VerificationStatus == VerificationStatus.Approved);
                    bool approvedCarLicense = allDocuments.Any(d => d.DocumentType == "CarLicense" && d.VerificationStatus == VerificationStatus.Approved);

                    int approvedCount = (approvedNationalId ? 1 : 0) + (approvedDrivingLicense ? 1 : 0) + (approvedCarLicense ? 1 : 0);

                    if (approvedNationalId && approvedDrivingLicense && approvedCarLicense)
                    {
                        if (user.UserRole != UserRole.Driver)
                        {
                            user.IsVerified = true;
                            user.UserRole = UserRole.Driver;
                            await _userRepository.UpdateUserAsync(user);

                            // Update car verification
                            var car = await _userRepository.GetUserCarAsync(user.Id);
                            if (car != null)
                            {
                                car.IsVerified = true;
                                await _userRepository.UpdateCarAsync(car);
                            }
                        }
                        message = "All driver documents approved. User is now a verified driver.";
                    }
                    else
                    {
                        message = $"Document approved successfully. {3 - approvedCount} documents remaining for driver verification.";
                    }
                }

                // Update the current verification
                await _userRepository.UpdateDocumentVerificationAsync(verification);
                
                return ApiResponse<bool>.SuccessResponse(true, message);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error handling document verification: {ex.Message}");
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
                    UserFullName = $"{v.User.FirstName} {v.User.LastName}",
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
    }
} 