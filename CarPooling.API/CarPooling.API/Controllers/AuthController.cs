using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarPooling.Application.Interfaces;
using System.Security.Claims;
using CarPooling.Domain.DTOs;

namespace CarPooling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
  
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<RegisterResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.RegisterAsync(request);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, ApiResponse<RegisterResponseDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.LoginAsync(request);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, ApiResponse<LoginResponseDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<string>>> RefreshToken([FromBody] string token)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<string>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.RefreshTokenAsync(token);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, ApiResponse<string>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("confirm-email")]
        public async Task<ActionResult<ApiResponse<string>>> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<string>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.ConfirmEmailAsync(userId, code);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email");
                return StatusCode(500, ApiResponse<string>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("resend-confirmation")]
        public async Task<ActionResult<ApiResponse<string>>> ResendConfirmationCode([FromBody] string email)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<string>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.ResendConfirmationCodeAsync(email);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending confirmation code");
                return StatusCode(500, ApiResponse<string>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] string email)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<string>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.ForgotPasswordAsync(email);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request");
                return StatusCode(500, ApiResponse<string>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<string>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.ResetPasswordAsync(request);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, ApiResponse<string>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
                }

                var result = await _authService.LogoutAsync(userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging out");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponse<CurrentUserDto>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
                var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
                var lastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

                var userInfo = new CurrentUserDto
                {
                    UserId = userId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = role
                };

                return Ok(ApiResponse<CurrentUserDto>.SuccessResponse(userInfo, "User information retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user info");
                return StatusCode(500, ApiResponse<CurrentUserDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("validate-token")]
        [Authorize]
        public ActionResult<ApiResponse<bool>> ValidateToken()
        {
            try
            {
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Token is valid"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }
    }
}