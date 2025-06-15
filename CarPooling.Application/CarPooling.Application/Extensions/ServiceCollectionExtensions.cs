using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            var appAssembly=typeof(ServiceCollectionExtensions).Assembly;

            services.AddMediatR(cfg=>cfg.RegisterServicesFromAssemblies(appAssembly));
            services.AddAutoMapper(appAssembly);



        }
    }
}
