using CarPooling.Application.Trips;
using CarPooling.Application.Trips.Validators;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Services;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Application.Trips.Commands.CreateRequest;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CarPooling.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            var appAssembly = typeof(ServiceCollectionExtensions).Assembly;

            services.AddScoped<IBookTripService, BookTripService>();
            services.AddScoped<ITripService, TripService>();
            services.AddAutoMapper(typeof(ServiceCollectionExtensions));

            // Register validators
            services.AddValidatorsFromAssemblyContaining<BookTripDtoValidator>();
            services.AddScoped<IValidator<CreateTripCommand>, CreateTripCommandValidator>();
            services.AddFluentValidationClientsideAdapters();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(appAssembly));

            // Register User Service
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDeliveryService, DeliveryService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IAdminService, AdminService>();
        }
    }
}