using AutoMapper;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Application.Trips.Commands.CreateRequest
{
    public class CreateTripCommandHandler : IRequestHandler<CreateTripCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTripCommandHandler> _logger;
        private readonly IValidator<CreateTripCommand> _validator;

        public CreateTripCommandHandler(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            ILogger<CreateTripCommandHandler> logger,
            IValidator<CreateTripCommand> validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<int> Handle(CreateTripCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validating trip creation request");
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Invalid trip creation request: {errors}");
            }

            _logger.LogInformation("Creating a new trip");
            var trip = _mapper.Map<Trip>(request);
            
            // Set CreatedAt to current UTC time
            trip.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Trips.CreateAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            return trip.TripId;
        }
    }
}
