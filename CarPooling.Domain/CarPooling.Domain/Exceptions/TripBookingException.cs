using System;

namespace CarPooling.Domain.Exceptions
{
    public class TripBookingException : Exception
    {
        public TripBookingException(string message) : base(message) { }

        public static TripBookingException UserNotFound() =>
            new TripBookingException("User not found");

        public static TripBookingException EmailNotConfirmed() =>
            new TripBookingException("Email not confirmed");

        public static TripBookingException UserNotVerified() =>
            new TripBookingException("User not verified");

        public static TripBookingException TripNotFound() =>
            new TripBookingException("Trip not found");

        public static TripBookingException TripNotAvailable() =>
            new TripBookingException("Trip is not available for booking");

        public static TripBookingException InsufficientSeats(int availableSeats) =>
            new TripBookingException($"Not enough seats available. Only {availableSeats} seats left");

        public static TripBookingException GenderPreferenceMismatch() =>
            new TripBookingException("Gender preference mismatch");

        public static TripBookingException AlreadyBooked() =>
            new TripBookingException("You have already booked this trip");

        public static TripBookingException BookingNotFound() =>
            new TripBookingException("Booking not found");

        public static TripBookingException AlreadyCancelled() =>
            new TripBookingException("Booking is already cancelled");
            
        public static TripBookingException DriverCannotBookOwnTrip() =>
            new TripBookingException("Drivers cannot book their own trips");
    }
}