using FluentValidation;
using InstrumentCatalogue.API.ReadModels;
using InstrumentCatalogue.Application.Exceptions;
using InstrumentCatalogue.Core.Exceptions;
using System.Net;

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

                    var errorDict = ex.Errors.GroupBy(x => x.PropertyName).ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToList());
                    var response = ApiResponse<object>.Fail(new ErrorDetail { Errors = errorDict, Message = ex.Message, TraceId = context.TraceIdentifier });


                    await context.Response.WriteAsJsonAsync(response);
                }

                catch (NotFoundException ex)
                {
                    _logger.LogWarning(ex, "NotFound exception for {path}", context.Request.Path);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    var response = ApiResponse<object>.Fail(new ErrorDetail { Message = ex.Message, TraceId = context.TraceIdentifier });

                    await context.Response.WriteAsJsonAsync(response);

                }

                catch (ConflictException ex)
                {
                    _logger.LogError(ex, "Conflict exception for {path}", context.Request.Path);
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    context.Response.ContentType = "application/json";
                    var response = ApiResponse<object>.Fail(new ErrorDetail { Message = ex.ClientMessage, TraceId = context.TraceIdentifier });

                    await context.Response.WriteAsJsonAsync(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception for {path}", context.Request.Path);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponse<object>.Fail(new ErrorDetail { Message = "An unhandled error occurred", TraceId = context.TraceIdentifier });

                    await context.Response.WriteAsJsonAsync(response);
                }
                }
}
