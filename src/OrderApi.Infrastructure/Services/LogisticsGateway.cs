using OrderApi.Core.Interfaces;

namespace OrderApi.Infrastructure.Services;

public class LogisticsGateway : ILogisticsGateway
{
    public async Task NotifyOrderCreatedAsync(int orderId)
    {
        // Simulate a 2-second delay for third-party service call
        await Task.Delay(2000);
    }
}
