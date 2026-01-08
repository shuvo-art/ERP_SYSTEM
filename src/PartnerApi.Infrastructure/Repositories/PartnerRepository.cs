using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using PartnerApi.Core.Entities;
using PartnerApi.Core.Interfaces;

namespace PartnerApi.Infrastructure.Repositories;

public class PartnerRepository : IPartnerRepository
{
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PartnerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> CreateAsync(Partner partner)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Name", partner.Name);
        parameters.Add("@Slug", partner.Slug);
        parameters.Add("@ShortDescription", partner.ShortDescription);
        parameters.Add("@LongDescriptionTitle", partner.LongDescriptionTitle);
        parameters.Add("@LongDescription", partner.LongDescription);
        parameters.Add("@LogoUrl", partner.LogoUrl);
        parameters.Add("@BuildingImageUrl", partner.BuildingImageUrl);
        parameters.Add("@CompanyProfileJson", JsonSerializer.Serialize(partner.CompanyProfile));
        parameters.Add("@ProductSegmentsJson", JsonSerializer.Serialize(partner.ProductSegments));
        parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreatePartner", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("@NewId");
    }

    public async Task<bool> UpdateAsync(Partner partner)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Id", partner.Id);
        parameters.Add("@Name", partner.Name);
        parameters.Add("@Slug", partner.Slug);
        parameters.Add("@ShortDescription", partner.ShortDescription);
        parameters.Add("@LongDescriptionTitle", partner.LongDescriptionTitle);
        parameters.Add("@LongDescription", partner.LongDescription);
        parameters.Add("@LogoUrl", partner.LogoUrl);
        parameters.Add("@BuildingImageUrl", partner.BuildingImageUrl);
        parameters.Add("@CompanyProfileJson", JsonSerializer.Serialize(partner.CompanyProfile));
        parameters.Add("@ProductSegmentsJson", JsonSerializer.Serialize(partner.ProductSegments));

        var rows = await connection.ExecuteAsync("sp_UpdatePartner", parameters, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<Partner?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var dynamicResult = await connection.QuerySingleOrDefaultAsync<dynamic>("sp_GetPartnerById", new { Id = id }, commandType: CommandType.StoredProcedure);
        return dynamicResult == null ? null : MapFromDynamic(dynamicResult);
    }

    public async Task<Partner?> GetBySlugAsync(string slug)
    {
        using var connection = new SqlConnection(_connectionString);
        var dynamicResult = await connection.QuerySingleOrDefaultAsync<dynamic>("sp_GetPartnerBySlug", new { Slug = slug }, commandType: CommandType.StoredProcedure);
        return dynamicResult == null ? null : MapFromDynamic(dynamicResult);
    }

    public async Task<(IEnumerable<Partner> Data, int Total)> GetAllAsync(string? search, int page, int limit)
    {
        using var connection = new SqlConnection(_connectionString);
        var offset = (page - 1) * limit;
        using var multi = await connection.QueryMultipleAsync("sp_GetPartners", new { Search = search, Offset = offset, Limit = limit }, commandType: CommandType.StoredProcedure);
        
        var dynamicData = await multi.ReadAsync<dynamic>();
        var partners = new List<Partner>();
        foreach (var d in dynamicData) partners.Add(MapFromDynamic(d));
        
        var total = await multi.ReadSingleAsync<int>();
        return (partners, total);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync("sp_DeletePartner", new { Id = id }, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    private Partner MapFromDynamic(dynamic d)
    {
        return new Partner
        {
            Id = d.Id,
            Name = d.Name,
            Slug = d.Slug,
            ShortDescription = d.ShortDescription,
            LongDescriptionTitle = d.LongDescriptionTitle,
            LongDescription = d.LongDescription,
            LogoUrl = d.LogoUrl,
            BuildingImageUrl = d.BuildingImageUrl,
            CompanyProfile = d.CompanyProfileJson != null ? JsonSerializer.Deserialize<Dictionary<string, string>>((string)d.CompanyProfileJson, _jsonOptions) ?? new() : new(),
            ProductSegments = d.ProductSegmentsJson != null ? JsonSerializer.Deserialize<List<ProductSegment>>((string)d.ProductSegmentsJson, _jsonOptions) ?? new() : new(),
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
    }
}
