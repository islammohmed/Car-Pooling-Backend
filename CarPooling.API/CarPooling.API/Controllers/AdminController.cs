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

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
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
    }
} 