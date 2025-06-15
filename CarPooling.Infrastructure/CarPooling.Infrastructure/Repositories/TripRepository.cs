using Car_Pooling.Data;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Infrastructure.Repositories
{
    internal class TripRepository(AppDbContext context) : ITripRepository
    {
        public async Task<int> create(Trip trip)
        {
            context.Trips.Add(trip);
            await context.SaveChangesAsync();
            return trip.TripId;
        }
    }
}
