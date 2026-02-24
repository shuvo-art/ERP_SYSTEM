using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Shared.Kernel.Middleware;

/// <summary>
/// Middleware-based distributed rate limiter using a Redis Sliding Window algorithm.
/// Protects sensitive endpoints: /register, /login, /forgot-password, /resend-otp.
/// 
/// How the Sliding Window works:
///   - Each request is recorded with its timestamp in a Redis Sorted Set (ZADD).
///   - Old entries outside the window are pruned (ZREMRANGEBYSCORE).
///   - The count of remaining entries = requests in the current window.
///   - If count > limit => 429 Too Many Requests.
///   - The key includes both IP and endpoint path for granular control.
/// </summary>
public class SlidingWindowRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SlidingWindowRateLimitMiddleware> _logger;
    private readonly List<RateLimitRule> _rules;

    // ─── Lua Script: Atomic Sliding Window ──────────────────────────────────
    // KEYS[1] = sorted set key (e.g. "rl:192.168.1.1:/api/v1/auth/login")
    // ARGV[1] = current timestamp (epoch ms)
    // ARGV[2] = window start timestamp (epoch ms)
    // ARGV[3] = window size in seconds (for EXPIRE TTL)
    // ARGV[4] = max allowed requests
    // ARGV[5] = unique request ID (epoch ms + random for uniqueness)
    //
    // Returns: { allowed (0/1), currentCount, retryAfterMs }
    private const string SlidingWindowLuaScript = @"
        -- 1. Remove entries outside the window
        redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', ARGV[2])

        -- 2. Count entries in the current window
        local currentCount = redis.call('ZCARD', KEYS[1])

        -- 3. Check if under the limit
        local maxRequests = tonumber(ARGV[4])
        if currentCount < maxRequests then
            -- 4a. Under limit: add this request and allow
            redis.call('ZADD', KEYS[1], ARGV[1], ARGV[5])
            -- Set TTL so the key auto-expires after the window
            redis.call('EXPIRE', KEYS[1], tonumber(ARGV[3]))
            return {1, currentCount + 1, 0}
        else
            -- 4b. Over limit: find the oldest entry to calculate retry-after
            local oldest = redis.call('ZRANGE', KEYS[1], 0, 0, 'WITHSCORES')
            local retryAfter = 0
            if #oldest > 0 then
                retryAfter = tonumber(oldest[2]) + (tonumber(ARGV[3]) * 1000) - tonumber(ARGV[1])
                if retryAfter < 0 then retryAfter = 0 end
            end
            return {0, currentCount, retryAfter}
        end
    ";

    public SlidingWindowRateLimitMiddleware(
        RequestDelegate next,
        ILogger<SlidingWindowRateLimitMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _rules = LoadRules(configuration);
    }

    public async Task InvokeAsync(HttpContext context, IConnectionMultiplexer redis)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method.ToUpperInvariant();

        // Find a matching rule for this request
        var rule = _rules.FirstOrDefault(r => MatchesRule(r, method, path));

        if (rule == null)
        {
            // No rate limit rule for this endpoint — pass through
            await _next(context);
            return;
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"rl:{clientIp}:{rule.Endpoint}";

        try
        {
            var db = redis.GetDatabase();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var windowStartMs = now - (long)(rule.WindowSeconds * 1000);
            var requestId = $"{now}:{Guid.NewGuid():N}";

            var result = (long[]?)await db.ScriptEvaluateAsync(
                SlidingWindowLuaScript,
                new RedisKey[] { key },
                new RedisValue[]
                {
                    now,                    // ARGV[1]: current timestamp
                    windowStartMs,          // ARGV[2]: window start
                    rule.WindowSeconds,     // ARGV[3]: window TTL
                    rule.MaxRequests,       // ARGV[4]: limit
                    requestId               // ARGV[5]: unique member
                }
            );

            if (result == null || result[0] == 0)
            {
                // Rate limit exceeded
                var retryAfterMs = result?[2] ?? (rule.WindowSeconds * 1000);
                var retryAfterSeconds = (int)Math.Ceiling(retryAfterMs / 1000.0);

                _logger.LogWarning(
                    "Rate limit exceeded for {ClientIp} on {Endpoint}. Current: {Count}/{Max}. Retry after {RetryAfter}s",
                    clientIp, rule.Endpoint, result?[1] ?? 0, rule.MaxRequests, retryAfterSeconds);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
                context.Response.Headers["X-RateLimit-Limit"] = rule.MaxRequests.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";

                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Too many requests. Please try again later.",
                    retryAfterSeconds
                });
                return;
            }

            // Allowed — set informational headers
            var remaining = rule.MaxRequests - (int)result[1];
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-RateLimit-Limit"] = rule.MaxRequests.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, remaining).ToString();
                return Task.CompletedTask;
            });

            await _next(context);
        }
        catch (RedisException ex)
        {
            // ─── Fail Open: if Redis is down, don't block the user ──────────────
            _logger.LogError(ex,
                "Redis unavailable for rate limiting on {Endpoint}. Failing open for {ClientIp}.",
                rule.Endpoint, clientIp);
            await _next(context);
        }
    }

    // ─── Configuration Loader ────────────────────────────────────────────────

    private static List<RateLimitRule> LoadRules(IConfiguration configuration)
    {
        var rules = new List<RateLimitRule>();
        var section = configuration.GetSection("SlidingWindowRateLimiting:Rules");

        if (section.Exists())
        {
            foreach (var child in section.GetChildren())
            {
                rules.Add(new RateLimitRule
                {
                    Endpoint = child["Endpoint"] ?? "",
                    HttpMethod = child["HttpMethod"] ?? "POST",
                    MaxRequests = int.Parse(child["MaxRequests"] ?? "5"),
                    WindowSeconds = int.Parse(child["WindowSeconds"] ?? "900")
                });
            }
        }

        // Defaults if no config is provided
        if (rules.Count == 0)
        {
            rules.AddRange(new[]
            {
                new RateLimitRule { Endpoint = "/api/v1/auth/register",        HttpMethod = "POST", MaxRequests = 5,  WindowSeconds = 3600 },
                new RateLimitRule { Endpoint = "/api/v1/auth/login",           HttpMethod = "POST", MaxRequests = 10, WindowSeconds = 900  },
                new RateLimitRule { Endpoint = "/api/v1/auth/forgot-password", HttpMethod = "POST", MaxRequests = 3,  WindowSeconds = 3600 },
                new RateLimitRule { Endpoint = "/api/v1/auth/resend-otp",      HttpMethod = "POST", MaxRequests = 3,  WindowSeconds = 3600 },
            });
        }

        return rules;
    }

    private static bool MatchesRule(RateLimitRule rule, string method, string path)
    {
        if (!string.Equals(rule.HttpMethod, method, StringComparison.OrdinalIgnoreCase)
            && rule.HttpMethod != "*")
            return false;

        return string.Equals(rule.Endpoint.ToLowerInvariant(), path, StringComparison.OrdinalIgnoreCase);
    }

    // ─── Inner Types ─────────────────────────────────────────────────────────

    private class RateLimitRule
    {
        public string Endpoint { get; set; } = "";
        public string HttpMethod { get; set; } = "POST";
        public int MaxRequests { get; set; } = 5;
        public int WindowSeconds { get; set; } = 900; // 15 minutes
    }
}
