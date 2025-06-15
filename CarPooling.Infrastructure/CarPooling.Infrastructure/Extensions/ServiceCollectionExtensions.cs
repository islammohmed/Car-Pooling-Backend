using Car_Pooling.Data;
using CarPooling.Domain.Repositories;
using CarPooling.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

           
            services.AddDbContext<AppDbContext>(options =>
              options.UseSqlServer(connectionString));


            services.AddScoped<ITripRepository, TripRepository>();
        }
    }
}
