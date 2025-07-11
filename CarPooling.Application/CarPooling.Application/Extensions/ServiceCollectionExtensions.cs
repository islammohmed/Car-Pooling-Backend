using CarPooling.Application.Interfaces;
using CarPooling.Application.Services;
using CarPooling.Application.Trips.DTOs;
using CarPooling.Application.Trips.Validators;
using CarPooling.Application.DTOs;
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

            // Register services
            services.AddScoped<IBookTripService, BookTripService>();
            services.AddScoped<ITripService, TripService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDeliveryService, DeliveryService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IAdminService, AdminService>();

            // Register AutoMapper
            services.AddAutoMapper(typeof(ServiceCollectionExtensions));

            // Register validators
            services.AddValidatorsFromAssemblyContaining<BookTripDtoValidator>();
            services.AddScoped<IValidator<CreateTripDto>, CreateTripDtoValidator>();
            services.AddFluentValidationClientsideAdapters();
        }
    }
}