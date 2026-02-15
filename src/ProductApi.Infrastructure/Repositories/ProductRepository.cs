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

    private async Task<IDbConnection> CreateConnectionAsync() => new SqlConnection(_connectionString);

    public async Task<int> CreateProductAsync(Product product)
    {
        using var connection = await CreateConnectionAsync();
        var parameters = GetParameterMap(product);
        parameters.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_CreateProduct", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("@NewProductId");
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        using var connection = await CreateConnectionAsync();
        var parameters = GetParameterMap(product);
        parameters.Add("@Id", product.Id);
        
        var rows = await connection.ExecuteAsync("sp_UpdateProduct", parameters, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        using var connection = await CreateConnectionAsync();
        using var multi = await connection.QueryMultipleAsync("sp_GetProductById", new { Id = id }, commandType: CommandType.StoredProcedure);

        var productData = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (productData == null) return null;

        var product = MapFromDynamic(productData);
        product.RelatedImages = (await multi.ReadAsync<string>()).ToList();

        return product;
    }

    public async Task<Product?> GetProductBySlugAsync(string slug)
    {
        using var connection = await CreateConnectionAsync();
        using var multi = await connection.QueryMultipleAsync("sp_GetProductById", new { Slug = slug }, commandType: CommandType.StoredProcedure);

        var productData = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (productData == null) return null;

        var product = MapFromDynamic(productData);
        product.RelatedImages = (await multi.ReadAsync<string>()).ToList();

        return product;
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllProductsAsync(int? categoryId, int? brandId, string? searchTerm, int pageNumber, int pageSize)
    {
        using var connection = await CreateConnectionAsync();
        var parameters = new { 
            CategoryId = categoryId, 
            BrandId = brandId, 
            SearchTerm = searchTerm, 
            PageNumber = pageNumber, 
            PageSize = pageSize 
        };

        var productsData = await connection.QueryAsync<dynamic>("sp_GetAllProducts", parameters, commandType: CommandType.StoredProcedure);
        
        var products = new List<Product>();
        int totalCount = 0;

        foreach (var d in productsData)
        {
            if (totalCount == 0) totalCount = d.TotalCount;
            products.Add(MapFromDynamic(d));
        }

        return (products, totalCount);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        using var connection = await CreateConnectionAsync();
        var rows = await connection.ExecuteAsync("sp_DeleteProduct", new { Id = id }, commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    private DynamicParameters GetParameterMap(Product product)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Name", product.Name);
        parameters.Add("@Slug", product.Slug);
        parameters.Add("@ShortDescription", product.ShortDescription);
        parameters.Add("@MainImage", product.MainImage);
        
        parameters.Add("@CategoryId", product.CategoryId);
        parameters.Add("@SubCategoryId", product.SubCategoryId);
        parameters.Add("@BrandId", product.BrandId);
        parameters.Add("@UnitId", product.UnitId);
        parameters.Add("@CountryId", product.CountryId);

        parameters.Add("@OverviewHtml", product.OverviewHtml);
        parameters.Add("@AdvantageHtml", product.AdvantageHtml);
        parameters.Add("@ApplicationRangeHtml", product.ApplicationRangeHtml);
        parameters.Add("@PrecautionHtml", product.PrecautionHtml);
        
        parameters.Add("@SpecificationsJson", JsonSerializer.Serialize(product.Specifications));
        parameters.Add("@TechnicalDataSheetsJson", JsonSerializer.Serialize(product.TechnicalDataSheets));
        parameters.Add("@SafetyDataSheetsJson", JsonSerializer.Serialize(product.SafetyDataSheets));
        parameters.Add("@CertificatesJson", JsonSerializer.Serialize(product.Certificates));
        parameters.Add("@RelatedImagesJson", JsonSerializer.Serialize(product.RelatedImages));
        
        return parameters;
    }

    private Product MapFromDynamic(dynamic d)
    {
        var product = new Product
        {
            Id = d.Id,
            Name = d.Name,
            Slug = d.Slug,
            ShortDescription = d.ShortDescription,
            MainImage = d.MainImage,
            CategoryId = d.CategoryId,
            SubCategoryId = d.SubCategoryId,
            BrandId = d.BrandId,
            UnitId = d.UnitId,
            CountryId = d.CountryId,
            CategoryName = d.CategoryName,
            SubCategoryName = d.SubCategoryName,
            BrandName = d.BrandName,
            UnitName = d.UnitName,
            CountryName = d.CountryName,
            OverviewHtml = d.OverviewHtml,
            AdvantageHtml = d.AdvantageHtml,
            ApplicationRangeHtml = d.ApplicationRangeHtml,
            PrecautionHtml = d.PrecautionHtml,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };

        if (d.SpecificationsJson != null)
            product.Specifications = JsonSerializer.Deserialize<ProductSpecifications>((string)d.SpecificationsJson, _jsonOptions) ?? new();
        
        if (d.TechnicalDataSheetsJson != null)
            product.TechnicalDataSheets = JsonSerializer.Deserialize<List<ProductDocument>>((string)d.TechnicalDataSheetsJson, _jsonOptions) ?? new();

        if (d.SafetyDataSheetsJson != null)
            product.SafetyDataSheets = JsonSerializer.Deserialize<List<ProductDocument>>((string)d.SafetyDataSheetsJson, _jsonOptions) ?? new();

        if (d.CertificatesJson != null)
            product.Certificates = JsonSerializer.Deserialize<List<ProductDocument>>((string)d.CertificatesJson, _jsonOptions) ?? new();

        return product;
    }
}
