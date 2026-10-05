using System.Diagnostics;

namespace Carpool.API.Middleware;

/// <summary>
/// Logs one line per request with method, path, status, duration and correlation id
/// (spec §59). The line is written from a <c>Response.OnCompleted</c> callback so the status
/// code is the final one — including a status set by
/// <see cref="ExceptionHandlingMiddleware"/> further out in the pipeline.
///
/// Only the path is logged, never the query string or request body, so credentials, tokens
/// and request payloads never reach the log (spec §60).
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        context.Response.OnCompleted(() =>
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);

            // The CorrelationIdMiddleware scope has been disposed by the time this callback
            // runs, so re-establish it for this line.
            using (_logger.BeginScope(new Dictionary<string, object>
                   {
                       ["CorrelationId"] = context.GetCorrelationId(),
                   }))
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds:0.0} ms.",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    elapsed.TotalMilliseconds);
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
