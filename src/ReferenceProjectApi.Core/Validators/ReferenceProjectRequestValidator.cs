using FluentValidation;
using ReferenceProjectApi.Core.DTOs;
using ReferenceProjectApi.Core.Interfaces;

namespace ReferenceProjectApi.Core.Validators;

public class ReferenceProjectRequestValidator : AbstractValidator<ReferenceProjectRequest>
{
    public ReferenceProjectRequestValidator(IReferenceProjectRepository repository, ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.ProjectName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CategoryId).NotEmpty().MustAsync(async (id, cancellation) => await categoryRepository.ExistsAsync(id))
            .WithMessage("Category must exist.");
        
        RuleFor(x => x.GalleryImages)
            .Must(x => x != null && x.Count > 0)
            .WithMessage("At least 1 gallery image required.");

        RuleFor(x => x.HeroImage).Must(BeAValidImage).When(x => x.HeroImage != null).WithMessage("Invalid image type.");
        RuleForEach(x => x.GalleryImages).Must(BeAValidImage).When(x => x.GalleryImages != null).WithMessage("Invalid gallery image type.");
        RuleForEach(x => x.DetailImages).Must(BeAValidImage).When(x => x.DetailImages != null).WithMessage("Invalid detail image type.");

        RuleFor(x => x.ProductIdsJson)
            .CustomAsync(async (json, context, cancellation) => {
                if (string.IsNullOrEmpty(json)) return;

                try {
                    var ids = System.Text.Json.JsonSerializer.Deserialize<List<int>>(json);
                    if (ids != null && ids.Any()) {
                        var exists = await repository.ProductsExistAsync(ids);
                        if (!exists) {
                            context.AddFailure("One or more product IDs are invalid.");
                        }
                    }
                } catch {
                    context.AddFailure("ProductIdsJson must be a valid JSON array of numbers.");
                }
            });
    }

    private bool BeAValidImage(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null) return true;
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}
