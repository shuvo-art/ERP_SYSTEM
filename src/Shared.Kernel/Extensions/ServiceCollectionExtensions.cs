using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
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
            
        services.AddScoped<ICacheService, RedisCacheService>();
        
        return services;
    }
}
