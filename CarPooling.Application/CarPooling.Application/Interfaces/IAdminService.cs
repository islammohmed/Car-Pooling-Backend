using CarPooling.Application.Common.Results;
using CarPooling.Application.DTOs;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Interfaces
{
    public interface IAdminService
    {
        Task<Result<List<DocumentVerificationDto>>>GetPendingDocumentVerifications();
        Task<Result<bool>> HandleDocumentVerification(string adminId, DocumentVerificationActionDto actionDto);
        Task<Result<List<DocumentVerificationDto>>> GetUserDocumentVerifications(string userId);
    }
} 