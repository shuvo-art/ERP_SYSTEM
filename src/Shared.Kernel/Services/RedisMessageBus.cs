using StackExchange.Redis;
using Shared.Kernel.Interfaces;

namespace Shared.Kernel.Services;

/// <summary>
/// Implements IMessagePublisher using Redis Pub/Sub.
/// Publishing is done via IConnectionMultiplexer.GetSubscriber().PublishAsync().
/// This is the correct pattern because the Pub/Sub subscriber should NOT be obtained
/// from IDatabase but from ISubscriber, which is designed for this purpose.
/// </summary>
public class RedisMessageBus : IMessagePublisher
{
    private readonly ISubscriber _subscriber;

    public RedisMessageBus(IConnectionMultiplexer redis)
    {
        _subscriber = redis.GetSubscriber();
    }

    /// <summary>
    /// Publishes a message to a Redis Pub/Sub channel.
    /// All running service instances subscribed to this channel will receive it.
    /// </summary>
    public async Task PublishAsync(string channel, string message)
    {
        await _subscriber.PublishAsync(RedisChannel.Literal(channel), message);
    }
}
