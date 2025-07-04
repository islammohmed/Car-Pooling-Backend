using AutoMapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace CarPooling.Application.Trips.Commands.CreateRequest
{
    public class CreateTripCommandValidator : AbstractValidator<CreateTripCommand>
    {
        public CreateTripCommandValidator()
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

            RuleFor(x => x.Destination)
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

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid trip status.");

            RuleFor(x => x.GenderPreference)
                .IsInEnum()
                .WithMessage("Invalid gender preference.");
        }
    }
}
