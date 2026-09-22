using System.Diagnostics;

namespace CSharpApp.Api.Middleware;

public sealed class PerformanceMiddleware(
    ILogger<PerformanceMiddleware> logger
) : IMiddleware
{
    private readonly ILogger<PerformanceMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var startTimestamp = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);

            _logger.LogInformation(
                "Request {Method} {Path} took {ElapsedMs:0.0} ms (status {StatusCode})",
                context.Request.Method,
                context.Request.Path,
                elapsed.TotalMilliseconds,
                context.Response.StatusCode);
        }
    }
}
