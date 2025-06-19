using CarPooling.Data;
using CarPooling.Domain.Repositories;
using CarPooling.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CarPooling.Infrastructure.Seeders;

namespace CarPooling.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

           
            services.AddDbContext<AppDbContext>(options =>
              options.UseSqlServer(connectionString));

            services.AddScoped<ISeeder, Seeder>();
            services.AddScoped<ITripRepository, TripRepository>();
        }
    }
}
