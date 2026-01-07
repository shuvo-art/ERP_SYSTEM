using Dapper;
using Microsoft.Data.SqlClient;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using System.Data;
using System.Text.Json;

namespace ProductApi.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> CreateProductAsync(Product product)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = GetParameterMap(product);
        parameters.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreateProduct", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("@NewProductId");
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = GetParameterMap(product);
        parameters.Add("@Id", product.Id);
        
        var rows = await connection.ExecuteAsync("sp_UpdateProduct", parameters, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync("sp_GetProductById", new { Id = id }, commandType: CommandType.StoredProcedure);

        var productData = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (productData == null) return null;

        var product = MapFromDynamic(productData);
        product.RelatedImages = (await multi.ReadAsync<string>()).ToList();

        return product;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var productsData = await connection.QueryAsync<dynamic>("sp_GetAllProducts", commandType: CommandType.StoredProcedure);
        
        var products = new List<Product>();
        foreach (var d in productsData)
        {
            products.Add(MapFromDynamic(d));
        }
        return products;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync("sp_DeleteProduct", new { Id = id }, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    private DynamicParameters GetParameterMap(Product product)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Name", product.Name);
        parameters.Add("@Description", product.Description);
        parameters.Add("@MainImage", product.Image);
        parameters.Add("@Category", product.Category);
        parameters.Add("@SubCategory", product.SubCategory);
        parameters.Add("@Brand", product.Brand);
        parameters.Add("@ApplicationRange", product.ApplicationRange);
        
        parameters.Add("@OverviewJson", JsonSerializer.Serialize(product.Overview));
        parameters.Add("@AdvantagesJson", JsonSerializer.Serialize(product.Advantages));
        parameters.Add("@PrecautionsJson", JsonSerializer.Serialize(product.Precautions));
        parameters.Add("@DocumentsJson", JsonSerializer.Serialize(product.Documents));
        parameters.Add("@RelatedImagesJson", JsonSerializer.Serialize(product.RelatedImages));
        
        return parameters;
    }

    private Product MapFromDynamic(dynamic d)
    {
        var product = new Product
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            Image = d.Image,
            Category = d.Category,
            SubCategory = d.SubCategory,
            Brand = d.Brand,
            ApplicationRange = d.ApplicationRange,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };

        if (d.OverviewJson != null)
            product.Overview = JsonSerializer.Deserialize<Overview>((string)d.OverviewJson, _jsonOptions) ?? new();
        
        if (d.AdvantagesJson != null)
            product.Advantages = JsonSerializer.Deserialize<List<string>>((string)d.AdvantagesJson, _jsonOptions) ?? new();

        if (d.PrecautionsJson != null)
            product.Precautions = JsonSerializer.Deserialize<List<string>>((string)d.PrecautionsJson, _jsonOptions) ?? new();

        if (d.DocumentsJson != null)
            product.Documents = JsonSerializer.Deserialize<Dictionary<string, List<string>>>((string)d.DocumentsJson, _jsonOptions) ?? new();

        return product;
    }
}
