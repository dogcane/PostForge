using System.Net;
using PostForge.Application.Common.Exceptions;

namespace PostForge.Api.Middleware;

public class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, detail) = exception switch
        {
            DomainValidationException => ((int)HttpStatusCode.BadRequest, "One or more domain validation failures have occurred."),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, exception.Message),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized access."),
            _ => ((int)HttpStatusCode.InternalServerError, "An internal server error occurred.")
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var errors = exception is DomainValidationException validationEx
            ? validationEx.Errors
            : null;

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title = GetTitleForStatusCode(statusCode),
            status = statusCode,
            detail,
            instance = context.Request.Path,
            errors
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        404 => "Not Found",
        500 => "Internal Server Error",
        _ => "Error"
    };
}
