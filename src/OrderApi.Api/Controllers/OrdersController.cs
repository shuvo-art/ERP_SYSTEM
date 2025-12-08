using Microsoft.AspNetCore.Mvc;
using OrderApi.Api.DTOs;
using OrderApi.Core.Entities;
using OrderApi.Core.Interfaces;

namespace OrderApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogisticsGateway _logisticsGateway;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderRepository orderRepository, ILogisticsGateway logisticsGateway, ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _logisticsGateway = logisticsGateway;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Basic Validation (Data Annotations handle most, but request mentions distinct specific codes usually, keeping it simple with BadRequest for now)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Domain Validation (Example)
        if (request.Items == null || !request.Items.Any())
        {
            return BadRequest(new { ErrorCode = "EMPTY_ORDER", Message = "Order must contain at least one item." });
        }

        // Map DTO to Entity
        var order = new Order
        {
            CustomerId = request.CustomerId,
            TotalAmount = request.TotalAmount,
            RequestId = request.RequestId,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        try
        {
            // 1. Create Order (Async DB Call)
            // Idempotency: logic is in SP. If ID is returned, we assume success or existing.
            var orderId = await _orderRepository.CreateOrderAsync(order);

            // 2. Process Logistics (Async third-party call)
            // "Simulate call ... Ensure API remains responsive"
            // We should await this. If it's slow (2s), the client waits 2s. 
            // "Ensure API remains responsive" usually implies async/await so thread isn't blocked, 
            // OR fire-and-forget (BackgroundService).
            // However, typically for a transaction like this, if the user needs confirmation logic started, await is fine.
            // If "responsive" means "return immediately and process in background", that's a different pattern (Queue).
            // But "Simulate call ... that takes 2 seconds" suggests just testing async/await non-blocking I/O.
            // I will await it as per standard simple REST unless BackgroundQueue is requested.
            // "Ensure the API remains responsive (use async/await correctly)" -> This explicitly asks for correct usage of async/await, not necessarily background processing.
            // So I will await.
            
            await _logisticsGateway.NotifyOrderCreatedAsync(orderId);

            return Ok(new { OrderId = orderId, Status = "Created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order");
            return StatusCode(500, new { ErrorCode = "INTERNAL_ERROR", Message = "An error occurred while processing the order." });
        }
    }
}
