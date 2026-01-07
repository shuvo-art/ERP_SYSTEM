using Microsoft.AspNetCore.Builder;

namespace Shared.Kernel.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<Shared.Kernel.Middleware.SecurityHeadersMiddleware>();
    }
}
