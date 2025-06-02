using Car_Pooling.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Pooling.Models
{
    public class TripParticipant
    {
       [Key]
        public int TripParticipantId { get; set; }
        public int TripId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public JoinStatus Status { get; set; } 
        public DateTime JoinedAt { get; set; }
        [ForeignKey (nameof (TripId))]
        public Trip Trip { get; set; }
        [ForeignKey (nameof (UserId))]
        public User User { get; set; }
        [Required, Range (1, 10)]
        public int SeatCount { get; set; }


    }
}
