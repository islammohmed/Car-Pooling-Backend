using CarPooling.Application.Trips.DTOs;
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
                
            RuleFor(dto => dto.CancellationReason)
                .MaximumLength(500)
                .WithMessage("Cancellation reason cannot exceed 500 characters.");
        }

        private bool BeValidGuid(string userId)
        {
            return Guid.TryParse(userId, out _);
        }
    }
}