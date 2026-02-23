using System.Text.Json;
using StackExchange.Redis;
using Shared.Kernel.Interfaces;

namespace Shared.Kernel.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    // --- Task 2: Lua Script (Atomic Verify & Delete) ---
    // This script runs on the Redis server as a single atomic operation.
    // It checks the value at KEYS[1]. If it matches ARGV[1], it deletes the key and returns 1 (success).
    // If it does NOT match, it returns 0 (failure).
    // This prevents race conditions where two simultaneous requests could both read the same valid OTP.
    private const string AtomicVerifyAndDeleteLua = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end
    ";

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    // ─── Basic Operations ────────────────────────────────────────────────────

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        if (expiration.HasValue)
            await _db.StringSetAsync(key, json, expiration.Value);
        else
            await _db.StringSetAsync(key, json);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    // ─── Task 1: Atomic Increment (Rate Limiting) ────────────────────────────

    /// <summary>
    /// Atomically increments a counter key and sets an expiry ONLY on the first increment.
    /// This correctly preserves the sliding window: the expiry is anchored to the first failure,
    /// not reset on every subsequent one. Returns the new count.
    /// </summary>
    public async Task<long> IncrementAsync(string key, TimeSpan expiration)
    {
        // StringIncrementAsync is atomic. It returns the new value after increment.
        var newCount = await _db.StringIncrementAsync(key);

        // Only set the expiry when the key is first created (count == 1).
        // This is the industry-standard "fixed window" approach.
        // If we called KeyExpireAsync every time, we'd reset the lockout window on every attempt.
        if (newCount == 1)
        {
            await _db.KeyExpireAsync(key, expiration);
        }

        return newCount;
    }

    // ─── Task 2: Lua Scripting (Atomic Verify & Delete) ─────────────────────

    /// <summary>
    /// Executes a Lua script on the Redis server to atomically verify and consume an OTP.
    /// This is the correct solution to race conditions in OTP verification.
    /// Returns true if the OTP was valid and has been consumed (deleted).
    /// Returns false if the OTP was invalid or already consumed.
    /// </summary>
    public async Task<bool> AtomicVerifyAndDeleteAsync(string key, string expectedValue)
    {
        // The Lua script executes atomically on the Redis server.
        // No other command can run between the GET and DEL operations.
        // ScriptEvaluateAsync(string, RedisKey[], RedisValue[]) sends script + args in one round-trip.
        var result = (long?)await _db.ScriptEvaluateAsync(
            AtomicVerifyAndDeleteLua,
            new RedisKey[] { key },
            new RedisValue[] { expectedValue }
        );

        // Script returns 1 (count of deleted keys) on success, 0 on failure.
        return result == 1;
    }
}
