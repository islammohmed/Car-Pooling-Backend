using CarPooling.Application.DTOs;
using FluentValidation;
using System;

namespace CarPooling.Application.Trips.Validators
{
    public class CreateTripDtoValidator : AbstractValidator<CreateTripDto>
    {
        public CreateTripDtoValidator()
        {
            RuleFor(x => x.DriverId)
                .NotEmpty()
                .WithMessage("Driver ID is required.");

            RuleFor(x => x.PricePerSeat)
                .GreaterThan(0)
                .WithMessage("Price per seat must be greater than 0.");

            RuleFor(x => x.EstimatedDuration)
                .Must(duration => duration > TimeSpan.Zero)
                .WithMessage("Estimated duration must be greater than 0.");

            RuleFor(x => x.AvailableSeats)
                .InclusiveBetween(1, 50)
                .WithMessage("Available seats must be between 1 and 50.");

            RuleFor(x => x.SourceLocation)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Source location is required and cannot exceed 100 characters.");

            RuleFor(x => x.DestinationLocation)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Destination is required and cannot exceed 100 characters.");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("Start time is required.")
                .Must(startTime => startTime > DateTime.UtcNow)
                .WithMessage("Start time must be in the future.");

            RuleFor(x => x.TripDescription)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.TripDescription))
                .WithMessage("Trip description cannot exceed 500 characters.");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Notes cannot exceed 500 characters.");

            RuleFor(x => x.MaxDeliveryWeight)
                .InclusiveBetween(0.1f, 100f)
                .When(x => x.AcceptsDeliveries && x.MaxDeliveryWeight.HasValue)
                .WithMessage("Maximum delivery weight must be between 0.1 and 100 kg.");
        }
    }
} 