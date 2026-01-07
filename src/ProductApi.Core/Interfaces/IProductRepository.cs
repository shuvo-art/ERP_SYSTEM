using ProductApi.Core.Entities;

namespace ProductApi.Core.Interfaces;

public interface IProductRepository
{
    Task<int> CreateProductAsync(Product product);
    Task<Product?> GetProductByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<bool> DeleteProductAsync(int id);
    Task<bool> UpdateProductAsync(Product product);
}
