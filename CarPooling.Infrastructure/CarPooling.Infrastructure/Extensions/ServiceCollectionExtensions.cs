using CarPooling.Domain.Interfaces;
using CarPooling.Domain.Interfaces.Repositories;
using CarPooling.Infrastructure.Repositories;
using CarPooling.Infrastructure.Services;
using CarPooling.Infrastructure.Data;
using CarPooling.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CarPooling.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using CarPooling.Domain.Entities;

namespace CarPooling.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("CarPooling.Infrastructure");
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                }));

            // Configure Identity
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IDeliveryRequestRepository, DeliveryRequestRepository>();

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register infrastructure services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();
            
            // Configure and register CloudinaryService
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.AddScoped<IFileStorageService, CloudinaryService>();
        }
    }
}
