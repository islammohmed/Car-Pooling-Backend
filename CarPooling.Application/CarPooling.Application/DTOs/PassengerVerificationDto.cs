using Microsoft.AspNetCore.Http;

namespace CarPooling.Application.DTOs
{
    public class PassengerVerificationDto
    {
        public IFormFile? NationalIdImage { get; set; }
    }
} 