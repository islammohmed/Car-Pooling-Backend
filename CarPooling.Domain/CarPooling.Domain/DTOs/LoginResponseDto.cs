using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarPooling.Domain.Enums;

namespace CarPooling.Domain.DTOs
{
 
        public class LoginResponseDto
        {
            public string UserId { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public UserRole UserRole { get; set; }
            public string Token { get; set; }
            public DateTime TokenExpiration { get; set; }
            public bool IsVerified { get; set; }
            public bool IsEmailConfirmed { get; set; }
        }
    
}
