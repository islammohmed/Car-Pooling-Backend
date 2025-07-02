using Microsoft.AspNetCore.Http;

namespace CarPooling.Application.DTOs
{
    public class DriverRegistrationDto
    {
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public IFormFile? NationalIdImage { get; set; }
        public IFormFile? DrivingLicenseImage { get; set; }
        public IFormFile? CarLicenseImage { get; set; }
    }
} 