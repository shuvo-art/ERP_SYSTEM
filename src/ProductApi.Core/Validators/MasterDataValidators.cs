using FluentValidation;
using ProductApi.Core.DTOs;

namespace ProductApi.Core.Validators;

public class BrandRequestValidator : AbstractValidator<BrandRequest>
{
    public BrandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Brand name is required.")
            .MaximumLength(100).WithMessage("Brand name must not exceed 100 characters.");
    }
}

public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
    }
}

public class SubCategoryRequestValidator : AbstractValidator<SubCategoryRequest>
{
    public SubCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("SubCategory name is required.")
            .MaximumLength(100).WithMessage("SubCategory name must not exceed 100 characters.");
            
        RuleFor(x => x.CategoryIds).NotEmpty().WithMessage("At least one Category ID is required.");
    }
}

public class UnitRequestValidator : AbstractValidator<UnitRequest>
{
    public UnitRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Unit name is required.")
            .MaximumLength(50).WithMessage("Unit name must not exceed 50 characters.");
    }
}

public class CountryRequestValidator : AbstractValidator<CountryRequest>
{
    public CountryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Country name is required.")
            .MaximumLength(100).WithMessage("Country name must not exceed 100 characters.");
    }
}

public class SubCategoryPatchRequestValidator : AbstractValidator<SubCategoryPatchRequest>
{
    public SubCategoryPatchRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100).WithMessage("SubCategory name must not exceed 100 characters.");
    }
}
