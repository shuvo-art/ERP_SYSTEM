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
    public class DistributorRepository : IDistributorRepository
    {
        private readonly string _connectionString;

        public DistributorRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Guid> CreateAsync(Distributor distributor)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Name", distributor.Name);
            parameters.Add("@Address", distributor.Address);
            parameters.Add("@Phone", distributor.Phone);
            parameters.Add("@Country", distributor.Country);
            parameters.Add("@Email", distributor.Email);
            parameters.Add("@Website", distributor.Website);
            parameters.Add("@IsActive", distributor.IsActive);
            parameters.Add("@DisplayOrder", distributor.DisplayOrder);
            parameters.Add("@NewDistributorId", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("sp_CreateDistributor", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<Guid>("@NewDistributorId");
        }

        public async Task<IEnumerable<Distributor>> GetAllAsync(bool? isActive)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Distributor>("sp_GetDistributors", new { IsActive = isActive }, commandType: CommandType.StoredProcedure);
        }

        public async Task<Distributor?> GetByIdAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Distributor>("sp_GetDistributorById", new { Id = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateAsync(Distributor distributor)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new {
                Id = distributor.Id,
                Name = distributor.Name,
                Address = distributor.Address,
                Phone = distributor.Phone,
                Country = distributor.Country,
                Email = distributor.Email,
                Website = distributor.Website,
                IsActive = distributor.IsActive,
                DisplayOrder = distributor.DisplayOrder
            };
            var rowsAffected = await connection.ExecuteAsync("sp_UpdateDistributor", parameters, commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync("sp_DeleteDistributor", new { Id = id }, commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateOrderAsync(IEnumerable<Guid> order)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                int rank = 1;
                foreach (var id in order)
                {
                    await connection.ExecuteAsync(
                        "UPDATE [dbo].[Distributors] SET DisplayOrder = @Rank, UpdatedAt = GETUTCDATE() WHERE Id = @Id",
                        new { Rank = rank++, Id = id },
                        transaction: transaction
                    );
                }
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}
