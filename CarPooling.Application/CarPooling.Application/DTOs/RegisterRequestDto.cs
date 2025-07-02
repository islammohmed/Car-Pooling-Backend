using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarPooling.Domain.Enums;

namespace CarPooling.Application.DTOs
{
    public class RegisterRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public UserRole UserRole { get; set; }

        [Required, StringLength(14)]
        public string SSN { get; set; } = string.Empty;

        public string? DrivingLicenseImage { get; set; }
        public string? NationalIDImage { get; set; }
        public Gender? Gender { get; set; } 

    }
}
