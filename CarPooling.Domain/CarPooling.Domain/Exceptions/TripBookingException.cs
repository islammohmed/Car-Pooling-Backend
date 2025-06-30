

namespace CarPooling.Domain.Exceptions
{
    public class TripBookingException : Exception
    {
        public TripBookingException(string message) : base(message)
        {
        }

        public TripBookingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static TripBookingException UserNotFound() => 
            new TripBookingException("User account not found. Please verify your account information.");

        public static TripBookingException TripNotFound() => 
            new TripBookingException("Trip not found.");

        public static TripBookingException InsufficientSeats(int availableSeats) => 
            new TripBookingException($"Not enough seats available. Only {availableSeats} seats left.");

        public static TripBookingException AlreadyBooked() => 
            new TripBookingException("You have already booked this trip.");
        public static TripBookingException BookingNotFound() =>
    new TripBookingException("No booking found for this trip and user.");

        public static TripBookingException AlreadyCancelled() =>
            new TripBookingException("This booking has already been cancelled.");
    }
}