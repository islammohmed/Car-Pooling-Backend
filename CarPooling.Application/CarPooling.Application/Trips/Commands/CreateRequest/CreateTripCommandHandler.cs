using AutoMapper;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
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

        public CreateTripCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTripCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<int> Handle(CreateTripCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new trip");
            var trip = _mapper.Map<Trip>(request);

            await _unitOfWork.Trips.CreateAsync(trip);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return trip.TripId;
        }
    }
}
