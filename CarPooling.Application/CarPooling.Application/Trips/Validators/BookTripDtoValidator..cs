using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarPooling.Application.Trips.DTOs;
using FluentValidation;

namespace CarPooling.Application.Trips.Validators
{
    public class BookTripDtoValidator : AbstractValidator<BookTripDto>
    
    {
        public BookTripDtoValidator()
        {
            RuleFor(dto => dto.TripId)
                .GreaterThan(0)
                .WithMessage("Trip ID must be a positive number.");

            RuleFor(dto => dto.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.")
                .Must(BeValidGuid)
                .WithMessage("User ID must be a valid GUID format.");

            RuleFor(dto => dto.SeatCount)
                .GreaterThan(0)
                .WithMessage("Seat count must be at least 1.")
                .LessThanOrEqualTo(10)
                .WithMessage("Seat count cannot exceed 10 seats per booking.");
        }

        private bool BeValidGuid(string userId)
        {
            return Guid.TryParse(userId, out _);
        }
    
}
}
