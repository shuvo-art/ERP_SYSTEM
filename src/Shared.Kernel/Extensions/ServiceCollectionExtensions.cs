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
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        // Default to "redis" host name if running in docker-compose, or localhost if undefined
        // Ideally should be set in ConnectionStrings:Redis
        var redisConnection = configuration.GetConnectionString("Redis") ?? configuration["RedisSettings:ConnectionString"] ?? "redis:6379";
        
        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "ERP_";
        });
            
        services.AddScoped<ICacheService, RedisCacheService>();
        
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
