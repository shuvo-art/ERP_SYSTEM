using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using StackExchange.Redis;
using Shared.Kernel.Interfaces;

namespace Shared.Kernel.Services;

/// <summary>
/// Circuit Breaker-wrapped Redis Cache Service.
/// 
/// Uses Polly to protect the application from Redis outages:
///   - If Redis fails 3 times in 30 seconds, the circuit OPENS.
///   - While open, calls immediately return default values ("Fail Fast") instead of hanging.
///   - After 30 seconds, the circuit moves to "Half-Open" — one call is allowed through
///     to test if Redis is back. If it succeeds, the circuit CLOSES again.
///
/// IMPORTANT: This replaces the original RedisCacheService. All ICacheService consumers
/// automatically benefit from circuit-breaking without code changes.
/// </summary>
public class ResilientRedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<ResilientRedisCacheService> _logger;
    private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

    // ─── Lua Script: Atomic Verify & Delete (unchanged from original) ────────
    private const string AtomicVerifyAndDeleteLua = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end
    ";

    public ResilientRedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<ResilientRedisCacheService> logger)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
        _logger = logger;

        // ─── Circuit Breaker Policy ──────────────────────────────────────────
        // Break after 3 consecutive failures within a 30s sampling window.
        // Stay open for 30 seconds before allowing a test request (Half-Open).
        _circuitBreaker = Policy
            .Handle<RedisException>()
            .Or<RedisTimeoutException>()
            .Or<RedisConnectionException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,          // 50% failure rate triggers break
                samplingDuration: TimeSpan.FromSeconds(30),
                minimumThroughput: 3,            // Need at least 3 calls to evaluate
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, timespan) =>
                {
                    _logger.LogError(exception,
                        "⚡ Redis Circuit Breaker OPENED. Redis calls will fail fast for {Duration}s.",
                        timespan.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("✅ Redis Circuit Breaker CLOSED. Redis is healthy again.");
                },
                onHalfOpen: () =>
                {
                    _logger.LogWarning("🔄 Redis Circuit Breaker HALF-OPEN. Testing Redis connectivity...");
                }
            );
    }

    /// <summary>Current state of the circuit breaker (Closed, Open, HalfOpen).</summary>
    public CircuitState CircuitState => _circuitBreaker.CircuitState;

    // ─── Basic Operations (with Circuit Breaker) ─────────────────────────────

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                var value = await _db.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                    return default;

                return JsonSerializer.Deserialize<T>(value!);
            });
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning("Redis circuit is open. Returning default for key: {Key}", key);
            return default;
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis error on GetAsync for key: {Key}. Returning default.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                var json = JsonSerializer.Serialize(value);
                if (expiration.HasValue)
                    await _db.StringSetAsync(key, json, expiration.Value);
                else
                    await _db.StringSetAsync(key, json);
            });
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning("Redis circuit is open. Skipping SetAsync for key: {Key}", key);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis error on SetAsync for key: {Key}. Operation skipped.", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                await _db.KeyDeleteAsync(key);
            });
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning("Redis circuit is open. Skipping RemoveAsync for key: {Key}", key);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis error on RemoveAsync for key: {Key}. Operation skipped.", key);
        }
    }

    // ─── Atomic Increment (Rate Limiting) with Circuit Breaker ───────────────

    public async Task<long> IncrementAsync(string key, TimeSpan expiration)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                var newCount = await _db.StringIncrementAsync(key);

                if (newCount == 1)
                {
                    await _db.KeyExpireAsync(key, expiration);
                }

                return newCount;
            });
        }
        catch (BrokenCircuitException)
        {
            // ─── Fail Open for rate limiting: if Redis is down, allow the request
            // but return 0 so the caller doesn't block users.
            _logger.LogWarning(
                "Redis circuit is open. IncrementAsync returning 0 (fail open) for key: {Key}", key);
            return 0;
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex,
                "Redis error on IncrementAsync for key: {Key}. Returning 0 (fail open).", key);
            return 0;
        }
    }

    // ─── Atomic Verify & Delete (Lua Script) with Circuit Breaker ────────────

    public async Task<bool> AtomicVerifyAndDeleteAsync(string key, string expectedValue)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                var result = (long?)await _db.ScriptEvaluateAsync(
                    AtomicVerifyAndDeleteLua,
                    new RedisKey[] { key },
                    new RedisValue[] { expectedValue }
                );

                return result == 1;
            });
        }
        catch (BrokenCircuitException)
        {
            // ─── Fail-through to DB: return false so the caller falls back to DB verification
            _logger.LogWarning(
                "Redis circuit is open. AtomicVerifyAndDeleteAsync returning false (fail to DB) for key: {Key}", key);
            return false;
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex,
                "Redis error on AtomicVerifyAndDeleteAsync for key: {Key}. Returning false (fail to DB).", key);
            return false;
        }
    }
}
