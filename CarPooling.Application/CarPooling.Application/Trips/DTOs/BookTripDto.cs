using System.ComponentModel.DataAnnotations;
<<<<<<< HEAD

=======
>>>>>>> cfe46bee6df57510064209057ce19d1068710181
namespace CarPooling.Application.Trips.DTOs
{
    public class BookTripDto
    {

        public int TripId { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; } = string.Empty;
   
        [Required(ErrorMessage = "SeatCount is required.")]
        public int SeatCount { get; set; }
        public string Status { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
  
}
