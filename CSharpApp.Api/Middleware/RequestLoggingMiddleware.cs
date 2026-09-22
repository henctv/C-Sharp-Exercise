using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace CSharpApp.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    ILogger<RequestLoggingMiddleware> logger
) : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogInformation(
            "Handling {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        try
        {
            await next(context);

            _logger.LogInformation(
                "Handled {Method} {Path} with status {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception while handling {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Instance = context.Request.Path
                });
            }
        }
    }
}
