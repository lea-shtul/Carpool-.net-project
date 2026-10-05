namespace Carpool.API.Middleware;

/// <summary>
/// The single, consistent error body returned by <see cref="ExceptionHandlingMiddleware"/>
/// for every thrown exception (spec §54). Model-validation failures caught by
/// <c>[ApiController]</c> use the framework's RFC 7807 <c>ProblemDetails</c> instead — a
/// separate path that never reaches the middleware.
/// </summary>
/// <param name="StatusCode">The HTTP status code also set on the response.</param>
/// <param name="Message">
/// A safe, client-facing message. For mapped domain exceptions this is the exception's own
/// business message; for unexpected errors it is a generic string — internal details are
/// never exposed (spec §54).
/// </param>
/// <param name="CorrelationId">The request's correlation id, so a client report ties to the server logs (spec §55).</param>
public sealed record ErrorResponse(int StatusCode, string Message, string CorrelationId);
