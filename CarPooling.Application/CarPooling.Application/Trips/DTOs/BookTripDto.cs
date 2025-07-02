using System.ComponentModel.DataAnnotations;

namespace CarPooling.Application.Trips.DTOs
{
    public class BookTripDto
    {

        public int TripId { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; } = string.Empty;
   
        [Required(ErrorMessage = "SeatCount is required.")]
        public int SeatCount { get; set; }
    }
  
}
