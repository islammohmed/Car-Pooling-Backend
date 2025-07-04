using CarPooling.Application.Extensions;
using CarPooling.Infrastructure.Extensions;
using CarPooling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

        // Configure connection string from appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Configure Entity Framework Core with SQL Server
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("CarPooling.Infrastructure");
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }));

        // Configure Identity
        builder.Services.AddIdentity<User, IdentityRole>(options =>
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

        // Extension methods for Infrastructure, Application, and API layers
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddApiServices(builder.Configuration);
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