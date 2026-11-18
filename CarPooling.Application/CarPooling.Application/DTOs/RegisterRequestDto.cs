using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarPooling.Domain.Enums;

namespace CarPooling.Domain.DTOs
{
    public class RegisterRequestDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }

        [Required, StringLength(50)]
        public required string FirstName { get; set; }

        [Required, StringLength(50)]
        public required string LastName { get; set; }

        [Required, Phone]
        public required string PhoneNumber { get; set; }

        [Required]
        public UserRole UserRole { get; set; }

        [Required, StringLength(14)]
        public required string SSN { get; set; }

        public string? DrivingLicenseImage { get; set; }
        public string? National_ID_Image { get; set; }
    }
}
