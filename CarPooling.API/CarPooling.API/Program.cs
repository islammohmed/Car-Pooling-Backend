using CarPooling.Application.Extensions;
using CarPooling.API.Middleware;
using CarPooling.API.Extensions;
using CarPooling.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarPooling.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddControllers();

        // Extension methods for Application and API layers
        builder.Services.AddApplication();
        builder.Services.AddApiServices(builder.Configuration);
        
        // Infrastructure layer is configured last to respect Clean Architecture
        // This is done through a separate extension method in the Infrastructure project
        // which is referenced by the API project at runtime
        var infrastructureConfigMethod = Type.GetType("CarPooling.Infrastructure.Extensions.ServiceCollectionExtensions, CarPooling.Infrastructure")
            ?.GetMethod("AddInfrastructure");
        
        if (infrastructureConfigMethod != null)
        {
            infrastructureConfigMethod.Invoke(null, new object[] { builder.Services, builder.Configuration });
        }
        else
        {
            throw new InvalidOperationException("Could not find Infrastructure configuration method. Make sure the Infrastructure assembly is referenced.");
        }
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "openapi/{documentName}.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/openapi/v1.json", "CarPooling API v1");
                c.RoutePrefix = "swagger";
            });
        }

        // Add global exception handling middleware
        app.UseExceptionMiddleware();

        app.UseHttpsRedirection();
        app.UseCors("AllowAngularApp");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
} 