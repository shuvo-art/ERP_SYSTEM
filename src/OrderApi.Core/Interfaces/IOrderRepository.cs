using OrderApi.Core.Entities;

namespace OrderApi.Core.Interfaces;

public interface IOrderRepository
{
    /// <summary>
    /// Creates an order. Returns the new OrderId, or the existing OrderId if RequestId matches (idempotency).
    /// </summary>
    Task<int> CreateOrderAsync(Order order);
    
    Task<Order?> GetByIdAsync(int id);
}
