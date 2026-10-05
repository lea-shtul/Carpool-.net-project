namespace Carpool.API.Middleware;

/// <summary>
/// Gives every incoming request a correlation id (spec §55). Honours a caller-supplied
/// <c>X-Correlation-ID</c> header, otherwise generates one. The id is:
/// <list type="bullet">
///   <item><description>stored on <see cref="HttpContext.Items"/> (read via
///   <see cref="CorrelationIdExtensions.GetCorrelationId"/>);</description></item>
///   <item><description>pushed into an <see cref="ILogger"/> scope so every downstream log
///   line carries it (spec §55, §59);</description></item>
///   <item><description>echoed back on the response as <c>X-Correlation-ID</c>.</description></item>
/// </list>
///
/// Registered just inside <see cref="ExceptionHandlingMiddleware"/> so the id is available
/// to request/error logging, per the required order (spec §56).
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    internal const string ItemsKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var supplied)
                            && !string.IsNullOrWhiteSpace(supplied)
            ? supplied.ToString()
            : Guid.NewGuid().ToString("D");

        context.Items[ItemsKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { [ItemsKey] = correlationId }))
        {
            await _next(context);
        }
    }
}

/// <summary>Reads the correlation id set by <see cref="CorrelationIdMiddleware"/>.</summary>
public static class CorrelationIdExtensions
{
    /// <summary>
    /// The current request's correlation id, or a freshly generated one if the middleware
    /// has not run yet (e.g. an exception thrown before it in the pipeline).
    /// </summary>
    public static string GetCorrelationId(this HttpContext context) =>
        context.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var value) && value is string id
            ? id
            : Guid.NewGuid().ToString("D");
}
