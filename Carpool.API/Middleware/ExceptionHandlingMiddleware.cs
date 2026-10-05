using Carpool.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Carpool.API.Middleware;

/// <summary>
/// Global exception handler (spec §54, §71). Registered outermost so it catches everything
/// downstream. Expected domain failures are mapped to their HTTP status and returned as a
/// consistent <see cref="ErrorResponse"/>; anything unexpected becomes a generic <c>500</c>
/// with no internal detail leaked to the client (spec §54).
///
/// A <see cref="DbUpdateException"/> caused by a PostgreSQL unique-constraint violation
/// (SQL state 23505) is treated as a <c>409</c> — this covers the rare race where two
/// concurrent requests both pass a service-layer "already exists?" check and the database
/// index rejects the second insert (duplicate rating, duplicate active booking, duplicate
/// licence plate).
/// </summary>
public sealed class ExceptionHandlingMiddleware
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away — not an error condition (spec §58: Error is for
            // unexpected failures only).
            _logger.LogInformation(
                "Request {Method} {Path} was cancelled by the client (correlationId {CorrelationId}).",
                context.Request.Method, context.Request.Path, context.GetCorrelationId());

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 499; // client closed request
            }
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.GetCorrelationId();

        var (statusCode, message, isUnexpected) = Map(exception);

        // This middleware runs outside the CorrelationIdMiddleware scope, so re-establish it.
        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            if (isUnexpected)
            {
                // Full detail (including stack trace) to the log only — never to the client.
                _logger.LogError(
                    exception,
                    "Unhandled exception for {Method} {Path} -> 500.",
                    context.Request.Method, context.Request.Path);
            }
            else
            {
                // A handled, expected outcome (spec §58: Warning). No stack trace, no request body.
                _logger.LogWarning(
                    "{Method} {Path} -> {StatusCode}: {Reason}",
                    context.Request.Method, context.Request.Path, statusCode, exception.Message);
            }

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response had already started; the error body could not be written.");
                return;
            }
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ErrorResponse(statusCode, message, correlationId));
    }

    private static (int StatusCode, string Message, bool IsUnexpected) Map(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, exception.Message, false),
        UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message, false),
        ForbiddenException => (StatusCodes.Status403Forbidden, exception.Message, false),
        BusinessRuleException => (StatusCodes.Status400BadRequest, exception.Message, false),
        ConcurrencyConflictException => (StatusCodes.Status409Conflict, exception.Message, false),
        ConflictException => (StatusCodes.Status409Conflict, exception.Message, false),
        DomainException => (StatusCodes.Status400BadRequest, exception.Message, false),
        DbUpdateException dbUpdate when IsUniqueViolation(dbUpdate) => (
            StatusCodes.Status409Conflict,
            "The request conflicts with an existing resource.",
            false),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", true),
    };

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
