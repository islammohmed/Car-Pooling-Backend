﻿using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<string>> RefreshTokenAsync(string token);
        Task<ApiResponse<string>> ConfirmEmailAsync(string userId, string code);
        Task<ApiResponse<bool>> LogoutAsync(string userId);
        Task<ApiResponse<string>> ResendConfirmationCodeAsync(string email);
        Task<ApiResponse<string>> ForgotPasswordAsync(string email);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
