using System.ComponentModel.DataAnnotations;

namespace Car_Pooling.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, StringLength (50)]
        public string FirstName { get; set; }

        [Required, StringLength (50)]
        public string LastName { get; set; }

        [Required, EmailAddress, StringLength (100)]
        public string Email { get; set; }

        [Required, Phone, StringLength (15)]
        public string Phone { get; set; }

        [StringLength (10)]
        public string ConfirmNumber { get; set; }

        [Required, StringLength (100)]
        public string Password { get; set; }

        [Required]
        public UserRole UserRole { get; set; }

        [Required, StringLength (14)]
        public string SSN { get; set; }

        public string DrivingLicenseImage { get; set; }

        public string National_ID_Image { get; set; }

        [Range (0, 5)]
        public float AvgRating { get; set; }

        public bool IsVerified { get; set; }

    }
}
