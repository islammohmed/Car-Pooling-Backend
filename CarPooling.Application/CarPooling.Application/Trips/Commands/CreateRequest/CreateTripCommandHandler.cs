using AutoMapper;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Application.Trips.Commands.CreateRequest
{
    internal class CreateTripCommandHandler (
        ILogger<CreateTripCommandValidator> logger,
         IMapper mapper,
         ITripRepository tripRepository)
        : IRequestHandler<CreateTripCommand, int>
    {
        public async Task<int> Handle(CreateTripCommand request, CancellationToken cancellationToken)
    {
            logger.LogInformation($"Creating a new trip");
            var trip = mapper.Map<Trip>(request);

            int id = await tripRepository.create( trip);
            return id;
        }
}
}
