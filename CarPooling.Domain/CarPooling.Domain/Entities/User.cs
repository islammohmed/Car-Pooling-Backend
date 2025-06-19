using System.ComponentModel.DataAnnotations;
using CarPooling.Domain.Enums;
using Microsoft.AspNetCore.Identity;  

namespace CarPooling.Domain.Entities
{
    public class User : IdentityUser
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; }
        [Required, StringLength(50)]
        public string LastName { get; set; }
        [Required]
        public UserRole UserRole { get; set; }
        [Required, StringLength(14)]
        public string SSN { get; set; }

        public Gender Gender { get; set; }

        public string? DrivingLicenseImage { get; set; } 
        public string NationalIDImage { get; set; }
        [Range(0, 5)]
        public float AvgRating { get; set; }
        public bool IsVerified { get; set; }
        [StringLength(10)]
        public string ConfirmNumber { get; set; }
        public ICollection<Car> Cars { get; set; }
        public ICollection<TripParticipant> TripParticipations { get; set; }
    }
}