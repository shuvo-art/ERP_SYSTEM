using Dapper;
using Microsoft.Data.SqlClient;
using OrderApi.Core.Entities;
using OrderApi.Core.Interfaces;
using System.Data;
using System.Text.Json;

namespace OrderApi.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> CreateOrderAsync(Order order)
    {
        using var connection = new SqlConnection(_connectionString);
        
        // Serialize items to JSON to pass to Stored Procedure
        var orderItemsJson = JsonSerializer.Serialize(order.Items);

        var p = new DynamicParameters();
        p.Add("@CustomerId", order.CustomerId);
        p.Add("@TotalAmount", order.TotalAmount);
        p.Add("@RequestId", order.RequestId);
        p.Add("@OrderItemsJson", orderItemsJson);
        p.Add("@NewOrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreateOrder", p, commandType: CommandType.StoredProcedure);

        return p.Get<int>("@NewOrderId");
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        // Minimal implementation as Get is not the primary requirement
        using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT * FROM Orders WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Order>(sql, new { Id = id });
    }
}
