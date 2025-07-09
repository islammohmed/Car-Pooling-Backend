using System.ComponentModel.DataAnnotations;
using CarPooling.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CarPooling.Domain.Entities
{
    public class User : IdentityUser
    {
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
  
        public UserRole UserRole { get; set; }
        [StringLength(14)] 
        public string SSN { get; set; } = string.Empty;
        public Gender? Gender { get; set; }

        public string? DrivingLicenseImage { get; set; } 
        public string NationalIdImage { get; set; } = string.Empty;
        [Range(1, 5)]
        public float AvgRating { get; set; }
        public bool IsVerified { get; set; }
        [StringLength(10)]
        public string ConfirmNumber { get; set; } = string.Empty;
        public bool HasLoggedIn { get; set; } = false;
        public ICollection<Car> Cars { get; set; } = new List<Car>();
        public ICollection<TripParticipant> TripParticipations { get; set; } = new List<TripParticipant>();

    }
}