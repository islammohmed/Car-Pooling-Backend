using CarPooling.Domain.Enums;

namespace CarPooling.Application.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole UserRole { get; set; }
        public Gender Gender { get; set; }
        public float AvgRating { get; set; }
        public string? ProfileImage { get; set; }
        public bool HasLoggedIn { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBlocked { get; set; }
    }
} 