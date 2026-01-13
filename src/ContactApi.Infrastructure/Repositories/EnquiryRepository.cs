using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ContactApi.Core.Entities;
using ContactApi.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ContactApi.Infrastructure.Repositories
{
    public class EnquiryRepository : IEnquiryRepository
    {
        private readonly string _connectionString;

        public EnquiryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Guid> CreateAsync(Enquiry enquiry)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Type", enquiry.Type);
            parameters.Add("@Name", enquiry.Name);
            parameters.Add("@Designation", enquiry.Designation);
            parameters.Add("@Mobile", enquiry.Mobile);
            parameters.Add("@Email", enquiry.Email);
            parameters.Add("@Address", enquiry.Address);
            parameters.Add("@Country", enquiry.Country);
            parameters.Add("@CompanyName", enquiry.CompanyName);
            parameters.Add("@ProductId", enquiry.ProductId);
            parameters.Add("@Message", enquiry.Message);
            parameters.Add("@RequestCallBack", enquiry.RequestCallBack);
            parameters.Add("@AgreeDataProtection", enquiry.AgreeDataProtection);
            parameters.Add("@NewEnquiryId", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("sp_CreateEnquiry", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<Guid>("@NewEnquiryId");
        }

        public async Task<IEnumerable<Enquiry>> GetAllAsync(string? type, string? status, string? search, DateTime? dateFrom, DateTime? dateTo)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Type", type);
            parameters.Add("@Status", status);
            parameters.Add("@Search", search);
            parameters.Add("@DateFrom", dateFrom);
            parameters.Add("@DateTo", dateTo);

            return await connection.QueryAsync<Enquiry>("sp_GetEnquiries", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Enquiry?> GetByIdAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Enquiry>("sp_GetEnquiryById", new { Id = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status, string? adminNotes)
        {
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync("sp_UpdateEnquiry", new { Id = id, Status = status, AdminNotes = adminNotes }, commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }
    }
}
