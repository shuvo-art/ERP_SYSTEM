using Microsoft.AspNetCore.Http;
using Shared.Kernel.Interfaces; 

namespace Shared.Kernel.Middleware;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token))
        {
            var isBlacklisted = await cacheService.GetAsync<string>($"blacklist_{token}");
            if (isBlacklisted != null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked/logged out." });
                return;
            }
        }
        await _next(context);
    }
}