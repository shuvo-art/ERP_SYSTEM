using ProductApi.Core.Entities;

namespace ProductApi.Core.Interfaces;

public interface IProductRepository
{
    Task<int> CreateProductAsync(Product product);
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product?> GetProductBySlugAsync(string slug);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllProductsAsync(int? categoryId, int? brandId, string? searchTerm, int pageNumber, int pageSize);
    Task<bool> DeleteProductAsync(int id);
    Task<bool> UpdateProductAsync(Product product);
}
