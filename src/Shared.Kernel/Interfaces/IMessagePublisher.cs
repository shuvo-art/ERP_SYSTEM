namespace Shared.Kernel.Interfaces;

/// <summary>
/// Publishes messages to a named channel for distributed event notification.
/// Used by Redis Pub/Sub to notify all service instances of cache-invalidating events.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the specified Redis channel.
    /// All subscribers to that channel across all instances will receive the message.
    /// </summary>
    Task PublishAsync(string channel, string message);
}
