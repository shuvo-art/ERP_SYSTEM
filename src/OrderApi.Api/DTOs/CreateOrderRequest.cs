using System.ComponentModel.DataAnnotations;

namespace OrderApi.Api.DTOs;

public class CreateOrderRequest
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public Guid RequestId { get; set; } // Idempotency Key

    [Required]
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    [Required]
    public string ProductId { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal UnitPrice { get; set; }
}
