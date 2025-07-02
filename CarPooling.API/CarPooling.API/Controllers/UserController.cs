using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CarPooling.Application.DTOs;
using CarPooling.Application.Interfaces;

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

        [HttpPost("profile")]
        public async Task<ActionResult<ApiResponse<bool>>> CompleteProfile(
            [FromForm] EditUserProfileDto profileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _userService.CompleteUserProfile(userId, profileDto, profileDto.NationalIdImageFile);
            return result.Success ? Ok(result) : BadRequest(result);
        }

    
    }
}