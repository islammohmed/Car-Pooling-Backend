
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CarPooling.Domain.Enums;

namespace CarPooling.Domain.Entities
{
    public class TripParticipant
    {
        public int Id { get; set; }
        public JoinStatus Status { get; set; }
        public DateTime JoinedAt { get; set; }
        [Range(1, 10)]
        public int SeatCount { get; set; }


        public int TripId { get; set; }
        public string UserId { get; set; }
        public Trip Trip { get; set; }
        public User User { get; set; }
  
    }
}
