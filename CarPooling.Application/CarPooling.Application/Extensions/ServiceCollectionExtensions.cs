using CarPooling.Application.Trips;
using CarPooling.Application.Trips.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.Extensions.DependencyInjection;

namespace CarPooling.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            var appAssembly=typeof(ServiceCollectionExtensions).Assembly;
            services.AddScoped<IBookTripService, BookTripService>();

            services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

            // Register validators
            services.AddValidatorsFromAssemblyContaining<BookTripDtoValidator>();
            services.AddFluentValidationClientsideAdapters();


            services.AddMediatR(cfg=>cfg.RegisterServicesFromAssemblies(appAssembly));



        }
    }
}
