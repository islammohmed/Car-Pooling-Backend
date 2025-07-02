using Microsoft.AspNetCore.Http;

namespace CarPooling.Application.DTOs
{
    public class UpdateDocumentDto
    {
        public string DocumentType { get; set; } = string.Empty;
        public IFormFile DocumentFile { get; set; } = null!;
    }
} 