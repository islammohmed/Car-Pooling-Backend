using System.ComponentModel.DataAnnotations;

namespace CarPooling.Application.DTOs.AuthDto
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string Token { get; set; }
    }

    public class ConfirmEmailRequestDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Code { get; set; }
    }

    public class ResendConfirmationRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
    }

    public class CurrentUserDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public bool IsVerified { get; set; }
        public string? DisplayName => $"{FirstName} {LastName}".Trim();
    }

    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
    }
}