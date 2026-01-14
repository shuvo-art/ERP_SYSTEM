using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AboutUsApi.Core.Entities;
using AboutUsApi.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AboutUsApi.Infrastructure.Repositories
{
    public class AboutUsRepository : IAboutUsRepository
    {
        private readonly string _connectionString;

        public AboutUsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<(IEnumerable<AboutUsSection> Sections, IEnumerable<AboutUsItem> Items)> GetFullAboutUsAsync()
        {
            using var connection = CreateConnection();
            using var multi = await connection.QueryMultipleAsync("sp_GetAboutUs", commandType: CommandType.StoredProcedure);
            var sections = await multi.ReadAsync<AboutUsSection>();
            var items = await multi.ReadAsync<AboutUsItem>();
            return (sections, items);
        }

        public async Task<AboutUsSection?> GetSectionAsync(string sectionId)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AboutUsSection>(
                "SELECT * FROM AboutUsSections WHERE Id = @Id", 
                new { Id = sectionId });
        }

        public async Task<bool> UpdateSectionAsync(AboutUsSection section)
        {
            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                "sp_UpdateSection",
                new { section.Id, section.Title, section.Description, section.MetadataJson },
                commandType: CommandType.StoredProcedure);
            return affectedRows > 0;
        }

        public async Task<Guid> AddItemAsync(AboutUsItem item)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SectionId", item.SectionId);
            parameters.Add("@Title", item.Title);
            parameters.Add("@ShortDescription", item.ShortDescription);
            parameters.Add("@IconUrl", item.IconUrl);
            parameters.Add("@ImageUrl", item.ImageUrl);
            parameters.Add("@Date", item.Date);
            parameters.Add("@Designation", item.Designation);
            parameters.Add("@SocialLinksJson", item.SocialLinksJson);
            parameters.Add("@OrderIndex", item.OrderIndex);
            parameters.Add("@NewItemId", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("sp_AddAboutUsItem", parameters, commandType: CommandType.StoredProcedure);
            return parameters.Get<Guid>("@NewItemId");
        }

        public async Task<bool> UpdateItemAsync(AboutUsItem item)
        {
            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                "sp_UpdateAboutUsItem",
                new 
                { 
                    item.Id, 
                    item.Title, 
                    item.ShortDescription, 
                    item.IconUrl, 
                    item.ImageUrl, 
                    item.Date, 
                    item.Designation, 
                    item.SocialLinksJson, 
                    item.OrderIndex 
                },
                commandType: CommandType.StoredProcedure);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteItemAsync(Guid itemId)
        {
            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                "sp_DeleteAboutUsItem", 
                new { Id = itemId }, 
                commandType: CommandType.StoredProcedure);
            return affectedRows > 0;
        }

        public async Task<AboutUsItem?> GetItemByIdAsync(Guid itemId)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AboutUsItem>(
                "SELECT * FROM AboutUsItems WHERE Id = @Id", 
                new { Id = itemId });
        }
    }
}
