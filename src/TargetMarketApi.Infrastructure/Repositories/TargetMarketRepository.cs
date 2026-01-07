using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using TargetMarketApi.Core.Entities;
using TargetMarketApi.Core.Interfaces;

namespace TargetMarketApi.Infrastructure.Repositories;

public class TargetMarketRepository : ITargetMarketRepository
{
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TargetMarketRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> CreateAsync(TargetMarket market)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Name", market.Name);
        parameters.Add("@Description", market.Description);
        parameters.Add("@ImageUrl", market.ImageUrl);
        parameters.Add("@SubItemsJson", JsonSerializer.Serialize(market.SubItems));
        parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreateTargetMarket", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("@NewId");
    }

    public async Task<bool> UpdateAsync(TargetMarket market)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Id", market.Id);
        parameters.Add("@Name", market.Name);
        parameters.Add("@Description", market.Description);
        parameters.Add("@ImageUrl", market.ImageUrl);
        parameters.Add("@SubItemsJson", JsonSerializer.Serialize(market.SubItems));

        var rows = await connection.ExecuteAsync("sp_UpdateTargetMarket", parameters, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<TargetMarket?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var dynamicResult = await connection.QuerySingleOrDefaultAsync<dynamic>("sp_GetTargetMarketById", new { Id = id }, commandType: CommandType.StoredProcedure);
        
        return dynamicResult == null ? null : MapFromDynamic(dynamicResult);
    }

    public async Task<(IEnumerable<TargetMarket> Data, int Total)> GetAllAsync(string? search, int page, int limit)
    {
        using var connection = new SqlConnection(_connectionString);
        var offset = (page - 1) * limit;
        
        using var multi = await connection.QueryMultipleAsync("sp_GetTargetMarkets", new { Search = search, Offset = offset, Limit = limit }, commandType: CommandType.StoredProcedure);
        
        var dynamicData = await multi.ReadAsync<dynamic>();
        var dataList = new List<TargetMarket>();
        foreach (var d in dynamicData)
        {
            dataList.Add(MapFromDynamic(d));
        }
        
        var total = await multi.ReadSingleAsync<int>();
        return (dataList, total);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync("sp_DeleteTargetMarket", new { Id = id }, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    private TargetMarket MapFromDynamic(dynamic d)
    {
        return new TargetMarket
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            ImageUrl = d.ImageUrl,
            SubItems = d.SubItemsJson != null ? JsonSerializer.Deserialize<List<string>>((string)d.SubItemsJson, _jsonOptions) ?? new() : new(),
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
    }
}
