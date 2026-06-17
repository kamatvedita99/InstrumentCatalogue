using FluentValidation;
using System.Net;
using System.Text.Json;

namespace InstrumentCatalogue.API.Middleware;

public class GlobalExceptionMiddleware
{
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            catch (ValidationException ex)
            {
            _logger.LogWarning(ex, "Validation failed {path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors.Select(x => new
            {
                field = x.PropertyName,
                mesage = x.ErrorMessage
            });

            await context.Response.WriteAsync(JsonSerializer.Serialize(new {errors}));
        }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Unhandled exception for {path}", context.Request.Path);
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "An unhandled error occurred" }));
            }
        }
}
