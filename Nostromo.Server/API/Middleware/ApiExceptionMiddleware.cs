using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;
using System.Net;
using System.Net.Http;

namespace Nostromo.Server.API.Middleware
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            IResult result = exception switch
            {
                NotFoundException notFoundEx => ApiResults.NotFound(notFoundEx.Message),
                ArgumentException argEx => ApiResults.BadRequest(argEx.Message),
                UnauthorizedAccessException => ApiResults.BadRequest("Unauthorized access"),
                HttpRequestException httpEx => HandleHttpException(httpEx),
                _ => ApiResults.ServerError("An unexpected error occurred")
            };

            await result.ExecuteAsync(context);
        }

        private static IResult HandleHttpException(HttpRequestException httpEx)
        {
            var statusCode = httpEx.StatusCode ?? HttpStatusCode.InternalServerError;

            return statusCode switch
            {
                HttpStatusCode.NotFound => ApiResults.NotFound("Resource not found"),
                HttpStatusCode.Unauthorized => ApiResults.BadRequest("Unauthorized access"),
                HttpStatusCode.BadRequest => ApiResults.BadRequest("Bad request"),
                _ => ApiResults.ServerError($"External service error: {statusCode}")
            };
        }
    }

    // Extension method remains the same
    public static class ApiExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiExceptionHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiExceptionMiddleware>();
        }
    }
}