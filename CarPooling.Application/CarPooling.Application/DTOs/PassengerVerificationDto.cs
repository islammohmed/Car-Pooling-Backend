using Microsoft.AspNetCore.Http;

namespace CarPooling.Application.DTOs
{
    public class PassengerVerificationDto
    {
        public required IFormFile NationalIdImage { get; set; }
    }
} 