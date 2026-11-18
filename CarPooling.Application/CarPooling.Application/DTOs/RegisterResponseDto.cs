using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Domain.DTOs
{
    public class RegisterResponseDto
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
        public bool IsVerified { get; set; }
        public string? ConfirmNumber { get; set; }
    }
}
