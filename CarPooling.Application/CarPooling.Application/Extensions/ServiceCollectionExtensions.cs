using CarPooling.Application.Trips;
using Microsoft.Extensions.DependencyInjection;

namespace CarPooling.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            var appAssembly=typeof(ServiceCollectionExtensions).Assembly;
            services.AddScoped<IBookTripService, BookTripService>();


            services.AddMediatR(cfg=>cfg.RegisterServicesFromAssemblies(appAssembly));
            services.AddAutoMapper(appAssembly);



        }
    }
}
