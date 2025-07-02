using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;

namespace CarPooling.API.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.ApiDescription.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(IFormFile));

            if (fileParameters.Any())
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "multipart/form-data", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "object",
                                    Properties = context.ApiDescription.ActionDescriptor.Parameters
                                        .ToDictionary(
                                            p => p.Name ?? "",
                                            p => p.ParameterType == typeof(IFormFile) 
                                                ? new OpenApiSchema { Type = "string", Format = "binary" }
                                                : new OpenApiSchema { Type = "string" }
                                        )
                                }
                            }
                        }
                    }
                };
            }
        }
    }
} 