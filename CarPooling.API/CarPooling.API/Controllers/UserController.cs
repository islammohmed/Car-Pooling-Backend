using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CarPooling.Application.Interfaces;
using CarPooling.Domain.DTOs;
using CarPooling.Application.DTOs;

namespace CarPooling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("passenger/verify")]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyPassengerNationalId(
            [FromForm] PassengerVerificationDto verificationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _userService.VerifyPassengerNationalId(userId, verificationDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("document/update")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateDocument(
            [FromForm] UpdateDocumentDto updateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _userService.UpdateDocument(userId, updateDto.DocumentType, updateDto.DocumentFile);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("documents/status")]
        public async Task<ActionResult<ApiResponse<List<DocumentVerificationDto>>>> GetDocumentVerifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _userService.GetUserDocumentVerifications(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("driver/register")]
        public async Task<ActionResult<ApiResponse<bool>>> RegisterAsDriver(
            [FromForm] DriverRegistrationDto driverDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _userService.RegisterAsDriver(userId, driverDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}