using System.Text;
using CarPooling.Application.Extensions;
using CarPooling.Infrastructure.Extensions;
using CarPooling.Infrastructure.Data;
using CarPooling.Application.Services;
using CarPooling.Domain.Entities;
using CarPooling.Application.Interfaces;
using CarPooling.Infrastructure.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CarPooling.Infrastructure.Services;
using Microsoft.OpenApi.Models;
using CarPooling.Domain.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using CarPooling.Infrastructure.Settings;

namespace CarPooling.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        // Seed the database
        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
            await seeder.Seed();
        }

        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddControllers();

        // Configure OpenAPI/Swagger
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CarPooling API",
                Version = "v1",
                Description = "CarPooling API for managing carpooling services"
            });

            // Configure Swagger to handle file uploads
            c.OperationFilter<FileUploadOperationFilter>();

            // Add JWT Authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Configure file upload handling
            c.MapType<IFormFile>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            });

            // Add operation filter for file uploads
            c.OperationFilter<FileUploadOperationFilter>();

            // Configure form data handling
            c.RequestBodyFilter<FileUploadRequestBodyFilter>();
        });

        //connection string from appsettings.json
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

        // JWT Authentication
        var jwtSettings = builder.Configuration.GetSection("JWT");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Configure Cloudinary
        builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
        builder.Services.AddScoped<IFileStorageService, CloudinaryService>();

        // Application Services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<SignInManager<User>>();
        builder.Services.AddScoped<IUserService, UserService>();

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp", policy =>
            {
                policy.WithOrigins("http://localhost:4200") // Angular dev server
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });

            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Extension methods for Infrastructure and Application layers
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();
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

        app.UseHttpsRedirection();
        app.UseCors("AllowAngularApp");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var formFileMediaType = "multipart/form-data";
        var formFileType = typeof(IFormFile);

        if (context.ApiDescription.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor actionDescriptor)
        {
            var formFileParameters = actionDescriptor.Parameters
                .Where(p => formFileType.IsAssignableFrom(p.ParameterType) || 
                          (p.ParameterType.IsGenericType && p.ParameterType.GetGenericArguments().Any(t => formFileType.IsAssignableFrom(t))))
                .ToList();

            if (formFileParameters.Any())
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            formFileMediaType, new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "object",
                                    Properties = formFileParameters.ToDictionary(
                                        p => p.Name,
                                        _ => new OpenApiSchema
                                        {
                                            Type = "string",
                                            Format = "binary"
                                        }
                                    ),
                                    Required = new HashSet<string>(formFileParameters.Select(p => p.Name))
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}

public class FileUploadRequestBodyFilter : IRequestBodyFilter
{
    public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        if (context.BodyParameterDescription == null || 
            context.BodyParameterDescription.Type == null) return;

        var properties = context.BodyParameterDescription.Type.GetProperties()
            .Where(p => typeof(IFormFile).IsAssignableFrom(p.PropertyType))
            .ToList();

        if (!properties.Any()) return;

        var formDataContent = requestBody.Content.FirstOrDefault(x => 
            x.Key.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase));

        if (formDataContent.Value != null)
        {
            foreach (var property in properties)
            {
                formDataContent.Value.Schema.Properties[property.Name] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
        }
    }
}
