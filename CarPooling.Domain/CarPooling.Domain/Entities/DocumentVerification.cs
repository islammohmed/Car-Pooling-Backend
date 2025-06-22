using CarPooling.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CarPooling.Domain.Entities
{
    public class DocumentVerification
    {
        public int Id { get; set; }

        public DateTime VerifiedAt { get; set; }

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        public string? RejectionReason { get; set; }

        public string DocumentType { get; set; }

        // Foreign Keys
        public string UserId { get; set; }
        public string AdminId { get; set; }

        // Navigation Properties
        public User User { get; set; }
        public User Admin { get; set; }
    }
}
