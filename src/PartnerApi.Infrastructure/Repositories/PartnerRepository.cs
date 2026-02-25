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
        parameters.Add("@DetailsDescriptionTitle", partner.DetailsDescriptionTitle);
        parameters.Add("@DetailsDescription", partner.DetailsDescription);
        parameters.Add("@LogoUrl", partner.LogoUrl);
        parameters.Add("@BuildingImageUrl", partner.BuildingImageUrl);
        parameters.Add("@VideoUrl", partner.VideoUrl);
        parameters.Add("@CompanyName", partner.CompanyName);
        parameters.Add("@BrandName", partner.BrandName);
        parameters.Add("@EstablishedIn", partner.EstablishedIn);
        parameters.Add("@Website", partner.Website);
        
        // Pass children as JSON for the stored procedure to handle insertion into separate tables
        parameters.Add("@ProductSegmentsJson", JsonSerializer.Serialize(partner.ProductSegments.Select(s => new { s.Name, s.ImageUrl })));
        parameters.Add("@DocumentsJson", JsonSerializer.Serialize(partner.Documents.Select(d => new { d.Name, d.DocumentUrl })));
        
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
        parameters.Add("@DetailsDescriptionTitle", partner.DetailsDescriptionTitle);
        parameters.Add("@DetailsDescription", partner.DetailsDescription);
        parameters.Add("@LogoUrl", partner.LogoUrl);
        parameters.Add("@BuildingImageUrl", partner.BuildingImageUrl);
        parameters.Add("@VideoUrl", partner.VideoUrl);
        parameters.Add("@CompanyName", partner.CompanyName);
        parameters.Add("@BrandName", partner.BrandName);
        parameters.Add("@EstablishedIn", partner.EstablishedIn);
        parameters.Add("@Website", partner.Website);
        
        parameters.Add("@ProductSegmentsJson", JsonSerializer.Serialize(partner.ProductSegments.Select(s => new { s.Name, s.ImageUrl })));
        parameters.Add("@DocumentsJson", JsonSerializer.Serialize(partner.Documents.Select(d => new { d.Name, d.DocumentUrl })));

        var rows = await connection.ExecuteAsync("sp_UpdatePartner", parameters, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<Partner?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync("sp_GetPartnerById", new { Id = id }, commandType: CommandType.StoredProcedure);
        
        var partner = await multi.ReadSingleOrDefaultAsync<Partner>();
        if (partner != null)
        {
            partner.ProductSegments = (await multi.ReadAsync<ProductSegment>()).ToList();
            partner.Documents = (await multi.ReadAsync<PartnerDocument>()).ToList();
        }
        
        return partner;
    }

    public async Task<Partner?> GetBySlugAsync(string slug)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync("sp_GetPartnerBySlug", new { Slug = slug }, commandType: CommandType.StoredProcedure);
        
        var partner = await multi.ReadSingleOrDefaultAsync<Partner>();
        if (partner != null)
        {
            partner.ProductSegments = (await multi.ReadAsync<ProductSegment>()).ToList();
            partner.Documents = (await multi.ReadAsync<PartnerDocument>()).ToList();
        }
        
        return partner;
    }

    public async Task<(IEnumerable<Partner> Data, int Total)> GetAllAsync(string? search, int page, int limit)
    {
        using var connection = new SqlConnection(_connectionString);
        var offset = (page - 1) * limit;
        using var multi = await connection.QueryMultipleAsync("sp_GetPartners", new { Search = search, Offset = offset, Limit = limit }, commandType: CommandType.StoredProcedure);
        
        var partners = await multi.ReadAsync<Partner>();
        var total = await multi.ReadSingleAsync<int>();
        
        return (partners, total);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync("sp_DeletePartner", new { Id = id }, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }
}
