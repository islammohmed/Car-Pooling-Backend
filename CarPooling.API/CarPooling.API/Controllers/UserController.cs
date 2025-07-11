using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CarPooling.Application.Interfaces;
using CarPooling.Domain.DTOs;
using CarPooling.Application.DTOs;
using CarPooling.Domain.Enums;

namespace CarPooling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
        {
            try
            {
                var result = await _userService.GetAllUsersAsync();
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, ApiResponse<List<UserDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(string id)
        {
            try
            {
                var result = await _userService.GetUserByIdAsync(id);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID");
                return StatusCode(500, ApiResponse<UserDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("passenger/verify")]
        
        public async Task<ActionResult<ApiResponse<bool>>> VerifyPassengerNationalId(
            [FromForm] PassengerVerificationDto verificationDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
                }

                var result = await _userService.VerifyPassengerNationalId(userId, verificationDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying passenger national ID");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("document/update")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateDocument(
            [FromForm] UpdateDocumentDto updateDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
                }

                var result = await _userService.UpdateDocument(userId, updateDto.DocumentType, updateDto.DocumentFile);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("documents/status")]
        public async Task<ActionResult<ApiResponse<List<DocumentVerificationDto>>>> GetDocumentVerifications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<DocumentVerificationDto>>.ErrorResponse("User not authenticated"));
                }

                var result = await _userService.GetUserDocumentVerifications(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document verifications");
                return StatusCode(500, ApiResponse<List<DocumentVerificationDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("driver/verification-status")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<bool>>> GetDriverVerificationStatus()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
                }

                var result = await _userService.GetUserDocumentVerifications(userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                var documentVerifications = result.Data;
                
                // Check if all required documents are verified
                bool hasVerifiedNationalId = documentVerifications.Any(
                    doc => doc.DocumentType == "NationalId" && doc.Status == VerificationStatus.Approved
                );
                
                bool hasVerifiedDrivingLicense = documentVerifications.Any(
                    doc => doc.DocumentType == "DrivingLicense" && doc.Status == VerificationStatus.Approved
                );
                
                bool hasVerifiedCarLicense = documentVerifications.Any(
                    doc => doc.DocumentType == "CarLicense" && doc.Status == VerificationStatus.Approved
                );
                
                bool isVerified = hasVerifiedNationalId && hasVerifiedDrivingLicense && hasVerifiedCarLicense;
                
                return Ok(ApiResponse<bool>.SuccessResponse(isVerified, isVerified ? 
                    "All documents are verified" : 
                    "Some documents require verification"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver verification status");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("driver/register")]
        public async Task<ActionResult<ApiResponse<bool>>> RegisterAsDriver(
            [FromForm] DriverRegistrationDto driverDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
                }

                var result = await _userService.RegisterAsDriver(userId, driverDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering as driver");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }
    }
}