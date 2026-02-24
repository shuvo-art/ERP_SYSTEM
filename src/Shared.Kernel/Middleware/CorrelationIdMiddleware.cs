using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared.Kernel.Middleware;

/// <summary>
/// Correlation ID Middleware.
/// 
/// Assigns a unique Correlation ID to every incoming HTTP request. If the client sends
/// an X-Correlation-ID header, it is reused; otherwise a new GUID is generated.
/// 
/// The Correlation ID is:
///   1. Stored in HttpContext.TraceIdentifier (for ASP.NET Core internal use).
///   2. Added to the Serilog LogContext (via scope) so EVERY log entry for this request
///      automatically includes CorrelationId — no manual passing needed.
///   3. Returned in the response header so the client can reference it in support tickets.
///
/// This makes it trivial to trace a single request across microservices:
///   - Grep your logs for CorrelationId = "abc-123" and see every step.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    public const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        // 1. Extract or generate the Correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("D");
        }

        // 2. Set it on the HttpContext so other middleware / controllers can access it
        context.TraceIdentifier = correlationId;

        // 3. Add to response headers so the caller can reference it
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // 4. Push into ILogger scope — Serilog picks this up and enriches every log
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
