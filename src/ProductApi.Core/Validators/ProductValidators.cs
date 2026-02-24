using FluentValidation;
using ProductApi.Core.DTOs;

namespace ProductApi.Core.Validators;

public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Short description must not exceed 500 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).When(x => x.CategoryId.HasValue).WithMessage("Category ID must be valid.");

        RuleFor(x => x.SubCategoryId)
            .GreaterThan(0).When(x => x.SubCategoryId.HasValue).WithMessage("SubCategory ID must be valid.");
            
        RuleFor(x => x.BrandId)
            .GreaterThan(0).When(x => x.BrandId.HasValue).WithMessage("Brand ID must be valid.");
            
        RuleFor(x => x.UnitId)
            .GreaterThan(0).When(x => x.UnitId.HasValue).WithMessage("Unit ID must be valid.");
            
        RuleFor(x => x.CountryId)
            .GreaterThan(0).When(x => x.CountryId.HasValue).WithMessage("Country ID must be valid.");
    }
}

public class ProductPatchRequestValidator : AbstractValidator<ProductPatchRequest>
{
    public ProductPatchRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Short description must not exceed 500 characters.");
            
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).When(x => x.CategoryId.HasValue).WithMessage("Category ID must be valid.");

        RuleFor(x => x.SubCategoryId)
            .GreaterThan(0).When(x => x.SubCategoryId.HasValue).WithMessage("SubCategory ID must be valid.");
            
        RuleFor(x => x.BrandId)
            .GreaterThan(0).When(x => x.BrandId.HasValue).WithMessage("Brand ID must be valid.");
            
        RuleFor(x => x.UnitId)
            .GreaterThan(0).When(x => x.UnitId.HasValue).WithMessage("Unit ID must be valid.");
            
        RuleFor(x => x.CountryId)
            .GreaterThan(0).When(x => x.CountryId.HasValue).WithMessage("Country ID must be valid.");
    }
}
