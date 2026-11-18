using CarPooling.Application.Common.Results;
using CarPooling.Domain.DTOs;

namespace CarPooling.API.Extensions
{
    public static class ResultExtensions
    {
        public static ApiResponse<T> ToApiResponse<T>(this Result<T> result)
        {
            return new ApiResponse<T>
            {
                Success = result.IsSuccess,
                Message = result.Message,
                Errors = result.Errors,
                Data = result.Data
            };
        }
    }
}
