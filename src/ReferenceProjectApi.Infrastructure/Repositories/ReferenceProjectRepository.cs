using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using ReferenceProjectApi.Core.Entities;
using ReferenceProjectApi.Core.Interfaces;
using System.Text.Json;

namespace ReferenceProjectApi.Infrastructure.Repositories;

public class ReferenceProjectRepository : IReferenceProjectRepository
{
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ReferenceProjectRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new ArgumentNullException("Connection string not found.");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<ReferenceProject>> GetProjectsAsync(int page, int limit, string? status, bool? featured, string? search)
    {
        using var connection = CreateConnection();
        var parameters = new { Page = page, Limit = limit, Status = status, Featured = featured, Search = search };
        
        using var multi = await connection.QueryMultipleAsync("sp_GetReferenceProjects", parameters, commandType: CommandType.StoredProcedure);
        
        var entities = await multi.ReadAsync<ReferenceProjectEntity>();
        // The SP returns total count as second result set which we might use later, but for now we follow the interface.
        
        return entities.Select(MapToDomain);
    }

    public async Task<int> GetTotalCountAsync(string? status, bool? featured, string? search)
    {
        using var connection = CreateConnection();
        // The sp_GetReferenceProjects already returns count, but we need a separate way if we follow current interface
        // Or we can just call the count part of the logic.
        // For simplicity and consistency with current IReferenceProjectRepository interface:
        var sql = @"
            SELECT COUNT(*) FROM ReferenceProjects
            WHERE (@Status IS NULL OR Status = @Status)
            AND (@Featured IS NULL OR Featured = @Featured)
            AND (@Search IS NULL OR ProjectName LIKE '%' + @Search + '%' OR ShortDescription LIKE '%' + @Search + '%')";
            
        return await connection.ExecuteScalarAsync<int>(sql, new { Status = status, Featured = featured, Search = search });
    }

    public async Task<ReferenceProject?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<ReferenceProjectEntity>(
            "sp_GetReferenceProjectById", 
            new { Id = id }, 
            commandType: CommandType.StoredProcedure);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<ReferenceProject?> GetBySlugAsync(string slug)
    {
        using var connection = CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<ReferenceProjectEntity>(
            "sp_GetReferenceProjectBySlug", 
            new { Slug = slug }, 
            commandType: CommandType.StoredProcedure);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<int> CreateAsync(ReferenceProject project)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ProjectName", project.ProjectName);
        parameters.Add("@Slug", project.Slug);
        parameters.Add("@ShortDescription", project.ShortDescription);
        parameters.Add("@HeroImageUrl", project.HeroImageUrl);
        parameters.Add("@GalleryImagesJson", JsonSerializer.Serialize(project.GalleryImages));
        parameters.Add("@Location", project.Location);
        parameters.Add("@ProjectOverviewJson", JsonSerializer.Serialize(project.ProjectOverview));
        parameters.Add("@ProductsUsedJson", JsonSerializer.Serialize(project.ProductsUsed));
        parameters.Add("@Status", project.Status);
        parameters.Add("@StartDate", project.StartDate);
        parameters.Add("@CompletionDate", project.CompletionDate);
        parameters.Add("@Featured", project.Featured);
        parameters.Add("@NewProjectId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreateReferenceProject", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("@NewProjectId");
    }

    public async Task UpdateAsync(ReferenceProject project)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", project.Id);
        parameters.Add("@ProjectName", project.ProjectName);
        parameters.Add("@Slug", project.Slug);
        parameters.Add("@ShortDescription", project.ShortDescription);
        parameters.Add("@HeroImageUrl", project.HeroImageUrl);
        parameters.Add("@GalleryImagesJson", JsonSerializer.Serialize(project.GalleryImages));
        parameters.Add("@Location", project.Location);
        parameters.Add("@ProjectOverviewJson", JsonSerializer.Serialize(project.ProjectOverview));
        parameters.Add("@ProductsUsedJson", JsonSerializer.Serialize(project.ProductsUsed));
        parameters.Add("@Status", project.Status);
        parameters.Add("@StartDate", project.StartDate);
        parameters.Add("@CompletionDate", project.CompletionDate);
        parameters.Add("@Featured", project.Featured);

        await connection.ExecuteAsync("sp_UpdateReferenceProject", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync("sp_DeleteReferenceProject", new { Id = id }, commandType: CommandType.StoredProcedure);
    }

    private ReferenceProject MapToDomain(ReferenceProjectEntity entity)
    {
        return new ReferenceProject
        {
            Id = entity.Id,
            ProjectName = entity.ProjectName,
            Slug = entity.Slug,
            ShortDescription = entity.ShortDescription,
            HeroImageUrl = entity.HeroImageUrl,
            GalleryImages = string.IsNullOrEmpty(entity.GalleryImagesJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(entity.GalleryImagesJson, _jsonOptions) ?? new(),
            Location = entity.Location,
            ProjectOverview = string.IsNullOrEmpty(entity.ProjectOverviewJson)
                ? null
                : JsonSerializer.Deserialize<ProjectOverview>(entity.ProjectOverviewJson, _jsonOptions),
            ProductsUsed = string.IsNullOrEmpty(entity.ProductsUsedJson)
                ? new List<ProjectProduct>()
                : JsonSerializer.Deserialize<List<ProjectProduct>>(entity.ProductsUsedJson, _jsonOptions) ?? new(),
            Status = entity.Status,
            StartDate = entity.StartDate,
            CompletionDate = entity.CompletionDate,
            Featured = entity.Featured,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private class ReferenceProjectEntity
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string? HeroImageUrl { get; set; }
        public string? GalleryImagesJson { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? ProjectOverviewJson { get; set; }
        public string? ProductsUsedJson { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public bool Featured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

