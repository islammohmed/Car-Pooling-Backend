using System.ComponentModel.DataAnnotations;

namespace CarPooling.Application.DTOs.AuthDto
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }

    public class ConfirmEmailRequestDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class ResendConfirmationRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class CurrentUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string? DisplayName => $"{FirstName} {LastName}".Trim();
    }

    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}