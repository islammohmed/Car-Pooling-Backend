using CarPooling.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CarPooling.Application.DTOs
{
    public class EditUserProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public Gender? Gender { get; set; }
        public string? SSN { get; set; }
        public string? NationalIdImage { get; set; }
        public string? DrivingLicenseImage { get; set; }
        public IFormFile? NationalIdImageFile { get; set; }
        public IFormFile? DrivingLicenseImageFile { get; set; }
        
    }
} 