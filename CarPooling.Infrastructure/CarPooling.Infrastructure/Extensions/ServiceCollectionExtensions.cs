using CarPooling.Infrastructure.Data;
using CarPooling.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Application.Interfaces;

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
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDeliveryRequestRepository, DeliveryRequestRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
