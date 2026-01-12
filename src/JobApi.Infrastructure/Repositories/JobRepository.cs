using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using JobApi.Core.Entities;
using JobApi.Core.Interfaces;
using System.Text.Json;

namespace JobApi.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public JobRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new ArgumentNullException("Connection string not found.");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<JobPosting>> GetAllAsync(string? status, string? department, string? location)
    {
        using var connection = CreateConnection();
        var entities = await connection.QueryAsync<JobPostingEntity>(
            "sp_GetJobs", 
            new { Status = status, Department = department, Location = location }, 
            commandType: CommandType.StoredProcedure);
        
        return entities.Select(MapToDomain);
    }

    public async Task<JobPosting?> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<JobPostingEntity>(
            "sp_GetJobById", 
            new { Id = id }, 
            commandType: CommandType.StoredProcedure);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<JobPosting?> GetBySlugAsync(string slug)
    {
        using var connection = CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<JobPostingEntity>(
            "sp_GetJobBySlug", 
            new { Slug = slug }, 
            commandType: CommandType.StoredProcedure);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<Guid> CreateAsync(JobPosting job)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Title", job.Title);
        parameters.Add("@Slug", job.Slug);
        parameters.Add("@Department", job.Department);
        parameters.Add("@ExperienceYears", job.ExperienceYears);
        parameters.Add("@JobType", job.JobType);
        parameters.Add("@ContractType", job.ContractType);
        parameters.Add("@Location", job.Location);
        parameters.Add("@Description", job.Description);
        parameters.Add("@ResponsibilitiesJson", JsonSerializer.Serialize(job.Responsibilities));
        parameters.Add("@QualificationsJson", JsonSerializer.Serialize(job.Qualifications));
        parameters.Add("@SkillsJson", JsonSerializer.Serialize(job.Skills));
        parameters.Add("@Status", job.Status);
        parameters.Add("@ApplicationDeadline", job.ApplicationDeadline);
        parameters.Add("@IsFeatured", job.IsFeatured);
        parameters.Add("@BannerImageUrl", job.BannerImageUrl);
        parameters.Add("@NewJobId", dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreateJob", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NewJobId");
    }

    public async Task UpdateAsync(JobPosting job)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", job.Id);
        parameters.Add("@Title", job.Title);
        parameters.Add("@Slug", job.Slug);
        parameters.Add("@Department", job.Department);
        parameters.Add("@ExperienceYears", job.ExperienceYears);
        parameters.Add("@JobType", job.JobType);
        parameters.Add("@ContractType", job.ContractType);
        parameters.Add("@Location", job.Location);
        parameters.Add("@Description", job.Description);
        parameters.Add("@ResponsibilitiesJson", JsonSerializer.Serialize(job.Responsibilities));
        parameters.Add("@QualificationsJson", JsonSerializer.Serialize(job.Qualifications));
        parameters.Add("@SkillsJson", JsonSerializer.Serialize(job.Skills));
        parameters.Add("@Status", job.Status);
        parameters.Add("@ApplicationDeadline", job.ApplicationDeadline);
        parameters.Add("@IsFeatured", job.IsFeatured);
        parameters.Add("@BannerImageUrl", job.BannerImageUrl);

        await connection.ExecuteAsync("sp_UpdateJob", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync("sp_DeleteJob", new { Id = id }, commandType: CommandType.StoredProcedure);
    }

    private JobPosting MapToDomain(JobPostingEntity entity)
    {
        return new JobPosting
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            Department = entity.Department,
            ExperienceYears = entity.ExperienceYears,
            JobType = entity.JobType,
            ContractType = entity.ContractType,
            Location = entity.Location,
            Description = entity.Description,
            Responsibilities = string.IsNullOrEmpty(entity.ResponsibilitiesJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(entity.ResponsibilitiesJson, _jsonOptions) ?? new(),
            Qualifications = string.IsNullOrEmpty(entity.QualificationsJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(entity.QualificationsJson, _jsonOptions) ?? new(),
            Skills = string.IsNullOrEmpty(entity.SkillsJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(entity.SkillsJson, _jsonOptions) ?? new(),
            Status = entity.Status,
            ApplicationDeadline = entity.ApplicationDeadline,
            IsFeatured = entity.IsFeatured,
            BannerImageUrl = entity.BannerImageUrl,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private class JobPostingEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string ContractType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ResponsibilitiesJson { get; set; }
        public string? QualificationsJson { get; set; }
        public string? SkillsJson { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ApplicationDeadline { get; set; }
        public bool IsFeatured { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
