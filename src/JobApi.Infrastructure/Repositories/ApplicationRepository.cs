using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using JobApi.Core.Entities;
using JobApi.Core.Interfaces;
using System.Text.Json;

namespace JobApi.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApplicationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new ArgumentNullException("Connection string not found.");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<JobApplication>> GetAllAsync(Guid? jobId, string? status, string? search)
    {
        using var connection = CreateConnection();
        var entities = await connection.QueryAsync<JobApplicationEntity>(
            "sp_GetApplications", 
            new { JobId = jobId, Status = status, Search = search }, 
            commandType: CommandType.StoredProcedure);
        
        return entities.Select(MapToDomain);
    }

    public async Task<int> GetTotalCountAsync(Guid? jobId, string? status, string? search)
    {
        using var connection = CreateConnection();
        var sql = @"
            SELECT COUNT(*) FROM JobApplications
            WHERE (@JobId IS NULL OR JobId = @JobId)
            AND (@Status IS NULL OR Status = @Status)
            AND (@Search IS NULL OR FirstName LIKE '%' + @Search + '%' OR LastName LIKE '%' + @Search + '%' OR Email LIKE '%' + @Search + '%')";
            
        return await connection.ExecuteScalarAsync<int>(sql, new { JobId = jobId, Status = status, Search = search });
    }

    public async Task<JobApplication?> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();
        var entity = await connection.QuerySingleOrDefaultAsync<JobApplicationEntity>(
            "sp_GetApplicationById", 
            new { Id = id }, 
            commandType: CommandType.StoredProcedure);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<Guid> CreateAsync(JobApplication application)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@JobId", application.JobId);
        parameters.Add("@JobTitle", application.JobTitle);
        parameters.Add("@FirstName", application.FirstName);
        parameters.Add("@LastName", application.LastName);
        parameters.Add("@Email", application.Email);
        parameters.Add("@Phone", application.Phone);
        parameters.Add("@Address", application.Address);
        parameters.Add("@ExperienceJson", JsonSerializer.Serialize(application.Experience));
        parameters.Add("@EducationJson", JsonSerializer.Serialize(application.Education));
        parameters.Add("@ResumeUrl", application.ResumeUrl);
        parameters.Add("@CoverMessage", application.CoverMessage);
        parameters.Add("@NewApplicationId", dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreateApplication", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NewApplicationId");
    }

    public async Task UpdateStatusAsync(Guid id, string status, string? notes)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync(
            "sp_UpdateApplicationStatus", 
            new { Id = id, Status = status, Notes = notes }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync("sp_DeleteApplication", new { Id = id }, commandType: CommandType.StoredProcedure);
    }

    private JobApplication MapToDomain(JobApplicationEntity entity)
    {
        return new JobApplication
        {
            Id = entity.Id,
            JobId = entity.JobId,
            JobTitle = entity.JobTitle,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            Experience = string.IsNullOrEmpty(entity.ExperienceJson)
                ? new List<ApplicationExperience>()
                : JsonSerializer.Deserialize<List<ApplicationExperience>>(entity.ExperienceJson, _jsonOptions) ?? new(),
            Education = string.IsNullOrEmpty(entity.EducationJson)
                ? new List<ApplicationEducation>()
                : JsonSerializer.Deserialize<List<ApplicationEducation>>(entity.EducationJson, _jsonOptions) ?? new(),
            ResumeUrl = entity.ResumeUrl,
            CoverMessage = entity.CoverMessage,
            Status = entity.Status,
            Notes = entity.Notes,
            AppliedAt = entity.AppliedAt
        };
    }

    private class JobApplicationEntity
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ExperienceJson { get; set; }
        public string? EducationJson { get; set; }
        public string ResumeUrl { get; set; } = string.Empty;
        public string? CoverMessage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
