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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, IUserService userService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("documents/pending")]
        public async Task<ActionResult<ApiResponse<List<DocumentVerificationDto>>>> GetPendingDocuments()
        {
            var result = await _adminService.GetPendingDocumentVerifications();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("documents/user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<DocumentVerificationDto>>>> GetUserDocuments(string userId)
        {
            var result = await _adminService.GetUserDocumentVerifications(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("documents/verify")]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyDocument(
            [FromBody] DocumentVerificationActionDto actionDto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
            }

            var result = await _adminService.HandleDocumentVerification(adminId, actionDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("users/{userId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string userId)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("Admin not authenticated"));
                }

                var result = await _userService.DeleteUserAsync(userId, adminId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("users/{userId}/block")]
        public async Task<ActionResult<ApiResponse<bool>>> BlockUser(string userId)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("Admin not authenticated"));
                }

                var result = await _userService.BlockUserAsync(userId, adminId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking user");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("users/{userId}/unblock")]
        public async Task<ActionResult<ApiResponse<bool>>> UnblockUser(string userId)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("Admin not authenticated"));
                }

                var result = await _userService.UnblockUserAsync(userId, adminId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking user");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }
    }
} 