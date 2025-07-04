using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.Enums;
using FluentValidation;

namespace CarPooling.Application.Trips.Validators
{
    public class CancelTripDtoValidator : AbstractValidator<CancelTripDto>
    {
        public CancelTripDtoValidator()
        {
            RuleFor(dto => dto.TripId)
                .GreaterThan(0)
                .WithMessage("Trip ID must be a positive number.");

            RuleFor(dto => dto.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.")
                .Must(BeValidGuid)
                .WithMessage("User ID must be a valid GUID format.");

            RuleFor(dto => dto.Role)
                .IsInEnum()
                .WithMessage("Invalid user role.")
                .Must(role => role == UserRole.Driver || role == UserRole.Passenger)
                .WithMessage("Role must be either Driver or Passenger for trip cancellation.");
                
            RuleFor(dto => dto.CancellationReason)
                .NotEmpty()
                .WithMessage("Cancellation reason is required.")
                .MinimumLength(10)
                .WithMessage("Cancellation reason must be at least 10 characters long.")
                .MaximumLength(500)
                .WithMessage("Cancellation reason cannot exceed 500 characters.");
        }

        private bool BeValidGuid(string userId)
        {
            return Guid.TryParse(userId, out _);
        }
    }
}