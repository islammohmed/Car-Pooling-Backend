using CarPooling.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CarPooling.Domain.Entities
{
    public class DocumentVerification
    {
        public int Id { get; set; }

        public string DocumentImage { get; set; } = string.Empty;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public DateTime SubmissionDate { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string DocumentType { get; set; } = string.Empty;

        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public string? AdminId { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public User? Admin { get; set; }
    }
}
