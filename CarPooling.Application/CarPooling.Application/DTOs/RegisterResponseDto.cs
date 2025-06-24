using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Domain.DTOs
{
    public class RegisterResponseDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
        public bool IsVerified { get; set; }
        public string? ConfirmNumber { get; set; }
    }
}
