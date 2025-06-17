
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class DocumentVerification
    {
        [Key]
        public int ID { get; set; }

        public DateTime Submitted_At { get; set; }
        public DateTime Verified_At { get; set; }

        public string Verification_Status { get; set; }

        public string Rejection_Reason { get; set; }
        public string Document_Type { get; set; }

        [ForeignKey("User")]
        public string User_ID { get; set; }

        [ForeignKey("Admin")]
        public string Verified_By_Admin_ID { get; set; }
        public User Admin { get; set; }

        public User User { get; set; }
    }

}
