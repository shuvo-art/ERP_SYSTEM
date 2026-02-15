using ProductApi.Core.Entities;

namespace ProductApi.Core.Interfaces;

public interface IMasterDataRepository
{
    // Categories
    Task<int> CreateCategoryAsync(CategoryMaster category);
    Task<IEnumerable<CategoryMaster>> GetCategoriesAsync(string? searchTerm = null, int? id = null, string? slug = null);
    Task<bool> UpdateCategoryAsync(CategoryMaster category);
    Task<bool> DeleteCategoryAsync(int id);

    // SubCategories
    Task<int> CreateSubCategoryAsync(SubCategoryMaster subCategory);
    Task<IEnumerable<SubCategoryMaster>> GetSubCategoriesAsync(string? searchTerm = null, int? id = null, string? slug = null);
    Task<bool> UpdateSubCategoryAsync(SubCategoryMaster subCategory);
    Task<bool> DeleteSubCategoryAsync(int id);

    // Brands
    Task<int> CreateBrandAsync(BrandMaster brand);
    Task<IEnumerable<BrandMaster>> GetBrandsAsync(string? searchTerm = null, int? id = null, string? slug = null);
    Task<bool> UpdateBrandAsync(BrandMaster brand);
    Task<bool> DeleteBrandAsync(int id);

    // Units
    Task<int> CreateUnitAsync(UnitMaster unit);
    Task<IEnumerable<UnitMaster>> GetUnitsAsync(string? searchTerm = null, int? id = null);
    Task<bool> UpdateUnitAsync(UnitMaster unit);
    Task<bool> DeleteUnitAsync(int id);

    // Countries
    Task<int> CreateCountryAsync(CountryMaster country);
    Task<IEnumerable<CountryMaster>> GetCountriesAsync(string? searchTerm = null, int? id = null);
    Task<bool> UpdateCountryAsync(CountryMaster country);
    Task<bool> DeleteCountryAsync(int id);
}
