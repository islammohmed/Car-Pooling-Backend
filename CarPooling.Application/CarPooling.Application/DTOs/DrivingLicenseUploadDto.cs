using Microsoft.AspNetCore.Http;

namespace CarPooling.Application.DTOs
{
    public class DrivingLicenseUploadDto
    {
        public IFormFile? LicenseFile { get; set; }
    }
} 