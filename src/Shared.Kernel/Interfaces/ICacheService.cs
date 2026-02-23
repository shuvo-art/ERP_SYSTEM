namespace Shared.Kernel.Interfaces;

public interface ICacheService
{
    // Basic Get/Set/Remove
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);

    // --- Task 1: Atomic Increment (Rate Limiting & Lockout) ---
    /// <summary>
    /// Atomically increments the integer value at a key and sets an expiry on first increment.
    /// Returns the new value after incrementing.
    /// </summary>
    Task<long> IncrementAsync(string key, TimeSpan expiration);

    // --- Task 2: Lua Scripting (Atomic Verify & Delete) ---
    /// <summary>
    /// Atomically verifies the OTP at the given key matches the expected value,
    /// then deletes the key. Returns true if OTP matched and was consumed; false otherwise.
    /// This prevents race conditions where the same OTP could be used twice simultaneously.
    /// </summary>
    Task<bool> AtomicVerifyAndDeleteAsync(string key, string expectedValue);
}
