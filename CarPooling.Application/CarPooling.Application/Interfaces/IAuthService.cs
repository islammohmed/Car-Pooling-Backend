using CarPooling.Application.Common.Results;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Result<string>> RefreshTokenAsync(string token);
        Task<Result<string>> ConfirmEmailAsync(string userId, string code);
        Task<Result<bool>> LogoutAsync(string userId);
        Task<Result<string>> ResendConfirmationCodeAsync(string email);
        Task<Result<string>> ForgotPasswordAsync(string email);
        Task<Result<string>> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
