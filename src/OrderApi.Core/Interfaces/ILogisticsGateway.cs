namespace OrderApi.Core.Interfaces;

public interface ILogisticsGateway
{
    Task NotifyOrderCreatedAsync(int orderId);
}
