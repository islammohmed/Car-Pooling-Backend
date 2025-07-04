using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CarPooling.Application.DTOs
{
    public class DriverVerificationDto
    {
        public IFormFile? NationalIdImage { get; set; }
        public IFormFile? DrivingLicenseImage { get; set; }
        public IFormFile? CarLicenseImage { get; set; }

        [Required]
        [StringLength(50)]
        public string CarModel { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CarColor { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CarPlateNumber { get; set; } = string.Empty;
    }
} 