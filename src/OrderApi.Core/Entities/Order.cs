namespace OrderApi.Core.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid RequestId { get; set; }
    public string Status { get; set; } = "Received";
    public List<OrderItem> Items { get; set; } = new();
}
