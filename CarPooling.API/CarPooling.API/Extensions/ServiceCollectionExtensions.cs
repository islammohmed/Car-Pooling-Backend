using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CarPooling.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using CarPooling.Infrastructure.Settings;
using CarPooling.Domain.Interfaces;
using CarPooling.Infrastructure.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using CarPooling.Infrastructure.Data;

namespace CarPooling.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure OpenAPI/Swagger
            services.AddSwaggerGen(c =>
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

            // JWT Authentication
            var jwtSettings = configuration.GetSection("JWT");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

            services.AddAuthentication(options =>
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
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.AddScoped<IFileStorageService, CloudinaryService>();

            // Configure CORS
            services.AddCors(options =>
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
            var formFileMediaType = "multipart/form-data";
            var formFileType = typeof(IFormFile);

            if (context.BodyParameterDescription != null && 
                context.BodyParameterDescription.Type != null)
            {
                var properties = context.BodyParameterDescription.Type.GetProperties();
                var formFileProperties = properties
                    .Where(p => formFileType.IsAssignableFrom(p.PropertyType) || 
                              (p.PropertyType.IsGenericType && p.PropertyType.GetGenericArguments().Any(t => formFileType.IsAssignableFrom(t))))
                    .ToList();

                if (formFileProperties.Any() && requestBody.Content.ContainsKey(formFileMediaType))
                {
                    var schema = requestBody.Content[formFileMediaType].Schema;
                    foreach (var property in formFileProperties)
                    {
                        if (schema.Properties.ContainsKey(property.Name))
                        {
                            schema.Properties[property.Name] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            };
                        }
                    }
                }
            }
        }
    }
} 