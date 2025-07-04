using CarPooling.Application.DTOs;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Interfaces
{
    public interface IAdminService
    {
        Task<ApiResponse<List<DocumentVerificationDto>>> GetPendingDocumentVerifications();
        Task<ApiResponse<bool>> HandleDocumentVerification(string adminId, DocumentVerificationActionDto actionDto);
        Task<ApiResponse<List<DocumentVerificationDto>>> GetUserDocumentVerifications(string userId);
    }
} 