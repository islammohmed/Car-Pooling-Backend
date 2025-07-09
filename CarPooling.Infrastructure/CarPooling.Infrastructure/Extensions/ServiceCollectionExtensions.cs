using CarPooling.Infrastructure.Data;
using CarPooling.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Application.Interfaces;
using CarPooling.Infrastructure.Services;
using CarPooling.Domain.Interfaces;

namespace CarPooling.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Repository registrations
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDeliveryRequestRepository, DeliveryRequestRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEmailService, EmailService>();

            // Service registrations
            services.AddScoped<IJwtService, JwtService>();
        }
    }
}
