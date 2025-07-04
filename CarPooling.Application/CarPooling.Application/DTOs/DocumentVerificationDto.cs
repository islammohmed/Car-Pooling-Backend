using CarPooling.Domain.Enums;
using System.Text.Json.Serialization;

namespace CarPooling.Application.DTOs
{
    public class DocumentVerificationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentImage { get; set; } = string.Empty;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VerificationStatus Status { get; set; }
        public DateTime SubmissionDate { get; set; }
    }

    public class DocumentVerificationActionDto
    {
        public int DocumentId { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VerificationStatus NewStatus { get; set; }
        public string? RejectionReason { get; set; }
    }
} 