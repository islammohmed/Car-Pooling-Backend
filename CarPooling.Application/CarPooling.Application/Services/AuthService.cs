using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using CarPooling.Domain.Entities;
using AutoMapper;
using CarPooling.Application.Interfaces;
using System.Security.Claims;
using CarPooling.Domain.DTOs;
using Microsoft.Extensions.Configuration;

namespace CarPooling.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            ILogger<AuthService> logger,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return ApiResponse<RegisterResponseDto>.ErrorResponse("User with this email already exists");
                }

                // Check if SSN already exists
                var existingSSN = _userManager.Users.FirstOrDefault(u => u.SSN == request.SSN);
                if (existingSSN != null)
                {
                    return ApiResponse<RegisterResponseDto>.ErrorResponse("User with this SSN already exists");
                }

                // Create new user
                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    UserRole = request.UserRole,
                    SSN = request.SSN,
                    DrivingLicenseImage = request.DrivingLicenseImage ?? string.Empty,
                    NationalIdImage = request.National_ID_Image ?? string.Empty,
                    AvgRating = 0,
                    IsVerified = false,
                    ConfirmNumber = GenerateConfirmationNumber(),
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<RegisterResponseDto>.ErrorResponse("Registration failed", errors);
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);
                var tokenExpiration = _jwtService.GetTokenExpiration();

                var response = new RegisterResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token,
                    TokenExpiration = tokenExpiration,
                    IsVerified = user.IsVerified,
                    ConfirmNumber = user.ConfirmNumber
                };

                _logger.LogInformation("User {Email} registered successfully", request.Email);

                return ApiResponse<RegisterResponseDto>.SuccessResponse(response, "Registration successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration");
                return ApiResponse<RegisterResponseDto>.ErrorResponse("An error occurred during registration");
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                _logger.LogInformation("=== LOGIN DEBUG START ===");

                // Find user by email
                _logger.LogInformation("Step 1: Finding user by email: {Email}", request.Email);
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Step 1: User not found for email: {Email}", request.Email);
                    return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid email or password");
                }
                _logger.LogInformation("Step 1: User found - ID: {UserId}, Email: {Email}", user.Id, user.Email);

                // Check password
                _logger.LogInformation("Step 2: Checking password for user: {Email}", request.Email);
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} account locked out", request.Email);
                    return ApiResponse<LoginResponseDto>.ErrorResponse("Account is locked out due to multiple failed attempts");
                }
                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("User {Email} not allowed to sign in", request.Email);
                    return ApiResponse<LoginResponseDto>.ErrorResponse("Account is not allowed to sign in");
                }
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed login attempt for user {Email}", request.Email);
                    return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid email or password");
                }
                _logger.LogInformation("Step 2: Password check successful for user: {Email}", request.Email);

                // Debug user properties
                _logger.LogInformation("Step 3: User properties debug:");
                _logger.LogInformation("- User.Id: {UserId} (Type: {Type})", user.Id, user.Id?.GetType().Name);
                _logger.LogInformation("- User.Email: {Email} (Type: {Type})", user.Email, user.Email?.GetType().Name);
                _logger.LogInformation("- User.FirstName: {FirstName} (Type: {Type})", user.FirstName, user.FirstName?.GetType().Name);
                _logger.LogInformation("- User.LastName: {LastName} (Type: {Type})", user.LastName, user.LastName?.GetType().Name);
                _logger.LogInformation("- User.UserRole: {UserRole} (Type: {Type})", user.UserRole, user.UserRole.GetType().Name);
                _logger.LogInformation("- User.IsVerified: {IsVerified} (Type: {Type})", user.IsVerified, user.IsVerified.GetType().Name);
                _logger.LogInformation("- User.EmailConfirmed: {EmailConfirmed} (Type: {Type})", user.EmailConfirmed, user.EmailConfirmed.GetType().Name);

                // Generate JWT token
                _logger.LogInformation("Step 4: Generating JWT token for user: {Email}", request.Email);
                var token = _jwtService.GenerateToken(user);
                _logger.LogInformation("Step 4: JWT token generated successfully");

                _logger.LogInformation("Step 5: Getting token expiration");
                var tokenExpiration = _jwtService.GetTokenExpiration();
                _logger.LogInformation("Step 5: Token expiration: {TokenExpiration}", tokenExpiration);

                _logger.LogInformation("Step 6: Creating LoginResponseDto");
                var loginResponse = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserRole = user.UserRole,
                    Token = token,
                    TokenExpiration = tokenExpiration,
                    IsVerified = user.IsVerified,
                    IsEmailConfirmed = user.EmailConfirmed
                };
                _logger.LogInformation("Step 6: LoginResponseDto created successfully");

                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                _logger.LogInformation("=== LOGIN DEBUG END ===");

                return ApiResponse<LoginResponseDto>.SuccessResponse(loginResponse, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user login at step: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                return ApiResponse<LoginResponseDto>.ErrorResponse("An error occurred during login");
            }
        }

        public async Task<ApiResponse<string>> RefreshTokenAsync(string token)
        {
            try
            {
                var principal = _jwtService.ValidateToken(token);
                if (principal == null)
                {
                    return ApiResponse<string>.ErrorResponse("Invalid token");
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return ApiResponse<string>.ErrorResponse("Invalid token claims");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<string>.ErrorResponse("User not found");
                }

                var newToken = _jwtService.GenerateToken(user);
                return ApiResponse<string>.SuccessResponse(newToken, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return ApiResponse<string>.ErrorResponse("An error occurred during token refresh");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                // Sign out the user
                await _signInManager.SignOutAsync();

                _logger.LogInformation("User {UserId} logged out successfully", userId);

                return ApiResponse<bool>.SuccessResponse(true, "Logout successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user logout");
                return ApiResponse<bool>.ErrorResponse("An error occurred during logout");
            }
        }

        public async Task<ApiResponse<string>> ConfirmEmailAsync(string userId, string code)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<string>.ErrorResponse("User not found");
                }

                // Check if the confirmation number matches
                if (user.ConfirmNumber != code)
                {
                    return ApiResponse<string>.ErrorResponse("Invalid confirmation code");
                }

                // Confirm the email
                user.EmailConfirmed = true;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<string>.ErrorResponse("Failed to confirm email", errors);
                }

                _logger.LogInformation("Email confirmed for user {UserId}", userId);

                return ApiResponse<string>.SuccessResponse("Email confirmed successfully", "Email confirmation successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email confirmation");
                return ApiResponse<string>.ErrorResponse("An error occurred during email confirmation");
            }
        }

        public async Task<ApiResponse<string>> ResendConfirmationCodeAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return ApiResponse<string>.ErrorResponse("User not found");
                }

                if (user.EmailConfirmed)
                {
                    return ApiResponse<string>.ErrorResponse("Email is already confirmed");
                }

                // Generate new confirmation number
                user.ConfirmNumber = GenerateConfirmationNumber();
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<string>.ErrorResponse("Failed to generate new confirmation code", errors);
                }

                _logger.LogInformation("New confirmation code generated for user {Email}", email);

                return ApiResponse<string>.SuccessResponse(user.ConfirmNumber, "New confirmation code generated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resending confirmation code");
                return ApiResponse<string>.ErrorResponse("An error occurred while resending confirmation code");
            }
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal that the user doesn't exist for security reasons
                    return ApiResponse<string>.SuccessResponse("If your email is registered, you will receive a password reset code", "Password reset code sent");
                }

                // Generate password reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // You might want to send this via email service instead of returning it directly
                // For now, we'll return it directly
                _logger.LogInformation("Password reset token generated for user {Email}", email);

                return ApiResponse<string>.SuccessResponse(resetToken, "Password reset token generated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during forgot password");
                return ApiResponse<string>.ErrorResponse("An error occurred while processing your request");
            }
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<string>.ErrorResponse("User not found");
                }

                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<string>.ErrorResponse("Failed to reset password", errors);
                }

                _logger.LogInformation("Password reset successful for user {Email}", request.Email);

                return ApiResponse<string>.SuccessResponse("Password reset successful", "Password has been reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password reset");
                return ApiResponse<string>.ErrorResponse("An error occurred during password reset");
            }
        }

        private string GenerateConfirmationNumber()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}