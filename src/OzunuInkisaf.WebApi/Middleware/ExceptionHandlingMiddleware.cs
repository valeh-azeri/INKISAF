using System.Net;
using System.Text.Json;
using OzunuInkisaf.Application.Common.Exceptions;
using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.WebApi.Middleware;

/// <summary>
/// Maps the Application layer's exception types to sensible HTTP status
/// codes, so controllers never need try/catch boilerplate.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            var statusCode = ex switch
            {
                NotFoundException => HttpStatusCode.NotFound,
                ValidationAppException => HttpStatusCode.BadRequest,
                AuthenticationFailedException => HttpStatusCode.Unauthorized,
                ForbiddenAccessException => HttpStatusCode.Forbidden,
                ConflictException => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError,
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Gözlənilməz xəta");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var payload = new ApiError { Message = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
