using Dapper;
using Microsoft.Data.SqlClient;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using System.Data;

namespace ProductApi.Infrastructure.Repositories;

public class MasterDataRepository : IMasterDataRepository
{
    private readonly string _connectionString;

    public MasterDataRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    // Categories
    public async Task<int> CreateCategoryAsync(CategoryMaster category)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleAsync<int>("sp_ManageCategory", 
            new { Action = "CREATE", Name = category.Name, Slug = category.Slug, Image = category.Image }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CategoryMaster>> GetCategoriesAsync(string? searchTerm = null, int? id = null, string? slug = null)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<CategoryMaster>("sp_ManageCategory", 
            new { Action = "GET", SearchTerm = searchTerm, Id = id, Slug = slug }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateCategoryAsync(CategoryMaster category)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageCategory", 
            new { Action = "UPDATE", Id = category.Id, Name = category.Name, Slug = category.Slug, Image = category.Image }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageCategory", 
            new { Action = "DELETE", Id = id }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    // SubCategories
    public async Task<int> CreateSubCategoryAsync(SubCategoryMaster subCategory)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleAsync<int>("sp_ManageSubCategory", 
            new { Action = "CREATE", CategoryId = subCategory.CategoryId, Name = subCategory.Name, Slug = subCategory.Slug }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<SubCategoryMaster>> GetSubCategoriesAsync(string? searchTerm = null, int? id = null, string? slug = null)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<SubCategoryMaster>("sp_ManageSubCategory", 
            new { Action = "GET", SearchTerm = searchTerm, Id = id, Slug = slug }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateSubCategoryAsync(SubCategoryMaster subCategory)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageSubCategory", 
            new { Action = "UPDATE", Id = subCategory.Id, CategoryId = subCategory.CategoryId, Name = subCategory.Name, Slug = subCategory.Slug }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<bool> DeleteSubCategoryAsync(int id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageSubCategory", 
            new { Action = "DELETE", Id = id }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    // Brands
    public async Task<int> CreateBrandAsync(BrandMaster brand)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleAsync<int>("sp_ManageBrand", 
            new { Action = "CREATE", Name = brand.Name, Slug = brand.Slug, Logo = brand.Logo }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<BrandMaster>> GetBrandsAsync(string? searchTerm = null, int? id = null, string? slug = null)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<BrandMaster>("sp_ManageBrand", 
            new { Action = "GET", SearchTerm = searchTerm, Id = id, Slug = slug }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateBrandAsync(BrandMaster brand)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageBrand", 
            new { Action = "UPDATE", Id = brand.Id, Name = brand.Name, Slug = brand.Slug, Logo = brand.Logo }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<bool> DeleteBrandAsync(int id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageBrand", 
            new { Action = "DELETE", Id = id }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    // Units
    public async Task<int> CreateUnitAsync(UnitMaster unit)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleAsync<int>("sp_ManageUnit", 
            new { Action = "CREATE", Name = unit.Name }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<UnitMaster>> GetUnitsAsync(string? searchTerm = null, int? id = null)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<UnitMaster>("sp_ManageUnit", 
            new { Action = "GET", SearchTerm = searchTerm, Id = id }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateUnitAsync(UnitMaster unit)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageUnit", 
            new { Action = "UPDATE", Id = unit.Id, Name = unit.Name }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<bool> DeleteUnitAsync(int id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageUnit", 
            new { Action = "DELETE", Id = id }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    // Countries
    public async Task<int> CreateCountryAsync(CountryMaster country)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleAsync<int>("sp_ManageCountry", 
            new { Action = "CREATE", Name = country.Name }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CountryMaster>> GetCountriesAsync(string? searchTerm = null, int? id = null)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<CountryMaster>("sp_ManageCountry", 
            new { Action = "GET", SearchTerm = searchTerm, Id = id }, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateCountryAsync(CountryMaster country)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageCountry", 
            new { Action = "UPDATE", Id = country.Id, Name = country.Name }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<bool> DeleteCountryAsync(int id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("sp_ManageCountry", 
            new { Action = "DELETE", Id = id }, 
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }
}
