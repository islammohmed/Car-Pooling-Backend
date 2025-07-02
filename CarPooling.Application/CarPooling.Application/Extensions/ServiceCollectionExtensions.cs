using CarPooling.Application.Trips;
using CarPooling.Application.Trips.Validators;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Services;
using CarPooling.Application.Interfaces.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarPooling.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            var appAssembly = typeof(ServiceCollectionExtensions).Assembly;

            services.AddScoped<IBookTripService, BookTripService>();
            services.AddAutoMapper(typeof(ServiceCollectionExtensions));

            // Register validators
            services.AddValidatorsFromAssemblyContaining<BookTripDtoValidator>();
            services.AddFluentValidationClientsideAdapters();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(appAssembly));

            // Register User Service
            services.AddScoped<IUserService, UserService>();
        }
    }
}