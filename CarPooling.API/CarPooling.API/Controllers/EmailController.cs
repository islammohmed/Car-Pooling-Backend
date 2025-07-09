using Microsoft.AspNetCore.Mvc;
using CarPooling.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;

namespace CarPooling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly UserManager<CarPooling.Domain.Entities.User> _userManager;

        public EmailController(IEmailService emailService, IConfiguration config, UserManager<CarPooling.Domain.Entities.User> userManager)
        {
            _emailService = emailService;
            _config = config;
            _userManager = userManager;
        }

        [HttpPost("send-config")]
        public async Task<IActionResult> SendConfigEmail()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            if (user.EmailConfirmed)
                return BadRequest(new { message = "Email is already confirmed." });

            var code = new Random().Next(100000, 999999).ToString();
            user.ConfirmNumber = code;
            await _userManager.UpdateAsync(user);

            var subject = "Car Pooling App - Email Configuration Code";
            var body = $"Hello {user.FirstName},\n\nYour configuration code is: {code}\n\nPlease enter this code in the app to confirm your email address.";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return Ok(new { message = $"Configuration code sent to {user.Email}" });
        }

        [HttpPost("config")]
        public async Task<IActionResult> ConfirmConfigCode([FromBody] string code)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            if (user.ConfirmNumber == code)
            {
                user.EmailConfirmed = true;
                user.ConfirmNumber = string.Empty; // Clear the code after confirmation
                await _userManager.UpdateAsync(user);
                return Ok(new { message = "Email confirmed successfully." });
            }
            else
            {
                return BadRequest(new { message = "Invalid confirmation code." });
            }
        }

        [HttpPost("resend-config")]
        public async Task<IActionResult> ResendConfigEmail()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            if (user.EmailConfirmed)
                return BadRequest(new { message = "Email is already confirmed." });

            // Always generate and send a new code
            var code = new Random().Next(100000, 999999).ToString();
            user.ConfirmNumber = code;
            await _userManager.UpdateAsync(user);

            var subject = "Car Pooling App - Email Configuration Code (Resend)";
            var body = $"Hello {user.FirstName},\n\nYour new configuration code is: {code}\n\nPlease enter this code in the app to confirm your email address.";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return Ok(new { message = $"A new configuration code has been sent to {user.Email}" });
        }
    }
} 