using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Shared.Kernel.Interfaces;
using Shared.Kernel.Services;

namespace Shared.Kernel.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration, string? instanceName = null)
    {
        // Default to "redis" host name if running in docker-compose, or localhost if undefined
        var redisConnection = configuration.GetConnectionString("Redis") ?? "redis:6379";
        
        // Dynamic Instance Name: Priority: Configuration > Parameter > Default
        var prefix = configuration["RedisSettings:InstanceName"] ?? instanceName ?? "ERP_";
        
        // Ensure prefix ends with underscore for readability
        if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith("_"))
        {
            prefix += "_";
        }

        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = prefix;
        });
            
        services.AddScoped<ICacheService, RedisCacheService>();

        // --- Task 3: Register Pub/Sub Publisher ---
        // Singleton because IConnectionMultiplexer is singleton and ISubscriber is thread-safe.
        services.AddSingleton<IMessagePublisher, RedisMessageBus>();

        // --- Task 3: Register Pub/Sub Subscriber (Background Service) ---
        // This starts a long-lived background worker that listens for "user-updates" events
        // and invalidates the local cache on this instance, enabling distributed invalidation.
        services.AddHostedService<UserCacheInvalidationService>();
        
        return services;
    }

    public static IServiceCollection AddRedisRateLimiting(this IServiceCollection services)
    {
        // Add basic rate limiting services
        services.AddMemoryCache();
        
        // This tells the AspNetCoreRateLimit library to use Redis instead of Memory
        services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
        services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
        services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();

        // For distributed rate limiting with Redis, use RedisProcessingStrategy
        services.AddSingleton<IProcessingStrategy, RedisProcessingStrategy>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }
}
