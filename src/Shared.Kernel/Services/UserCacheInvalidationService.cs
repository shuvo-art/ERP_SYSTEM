using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Shared.Kernel.Interfaces;

namespace Shared.Kernel.Services;

/// <summary>
/// A background service that subscribes to the Redis "user-updates" Pub/Sub channel.
/// 
/// Problem it solves:
/// In a distributed system with multiple running instances of the Auth microservice,
/// simply calling _cacheService.RemoveAsync() only clears the cache on the CURRENT instance.
/// Other instances may still serve stale data (e.g. old roles).
///
/// Solution (Distributed Cache Invalidation):
/// 1. When a user is updated, we PUBLISH a userId to the "user-updates" channel.
/// 2. ALL running instances are subscribed to this channel via THIS background service.
/// 3. Each instance receives the message and clears ITS OWN local/distributed cache entry.
/// This ensures all instances stay in sync without direct coupling.
/// </summary>
public class UserCacheInvalidationService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserCacheInvalidationService> _logger;

    // The channel name used for publishing/subscribing user update events.
    public const string Channel = "user-updates";

    public UserCacheInvalidationService(
        IConnectionMultiplexer redis,
        ICacheService cacheService,
        ILogger<UserCacheInvalidationService> logger)
    {
        _redis = redis;
        _cacheService = cacheService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserCacheInvalidationService started. Subscribing to channel: '{Channel}'", Channel);

        var subscriber = _redis.GetSubscriber();

        // Subscribe to the "user-updates" channel.
        // The handler runs on a Redis thread pool thread, so we use an async lambda.
        await subscriber.SubscribeAsync(
            RedisChannel.Literal(Channel),
            async (channel, message) =>
            {
                if (message.IsNullOrEmpty)
                    return;

                var userId = message.ToString();
                var cacheKey = $"user_profile_{userId}";

                _logger.LogInformation(
                    "Received user-update event for UserId: {UserId}. Invalidating cache key: '{CacheKey}'",
                    userId, cacheKey);

                try
                {
                    await _cacheService.RemoveAsync(cacheKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to invalidate cache for UserId: {UserId}", userId);
                }
            });

        // Keep the service alive until the application stops.
        await Task.Delay(Timeout.Infinite, stoppingToken);

        // Graceful shutdown: unsubscribe cleanly.
        _logger.LogInformation("UserCacheInvalidationService stopping. Unsubscribing from '{Channel}'", Channel);
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(Channel));
    }
}
