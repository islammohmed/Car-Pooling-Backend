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

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
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

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<string>>> RefreshToken([FromBody] string token)
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

        [HttpPost("confirm-email")]
        public async Task<ActionResult<ApiResponse<string>>> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
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

        [HttpPost("resend-confirmation")]
        public async Task<ActionResult<ApiResponse<string>>> ResendConfirmationCode([FromBody] string email)
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

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] string email)
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

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
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

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.LogoutAsync(userId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponse<CurrentUserDto>> GetCurrentUser()
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

        [HttpGet("validate-token")]
        [Authorize]
        public ActionResult<ApiResponse<bool>> ValidateToken()
        {
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Token is valid"));
        }
    }
}