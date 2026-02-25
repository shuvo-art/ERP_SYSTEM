using FluentValidation;
using PartnerApi.Core.DTOs;

namespace PartnerApi.Core.Validators;

public class PartnerRequestValidator : AbstractValidator<PartnerRequest>
{
    public PartnerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).MaximumLength(200);
        RuleFor(x => x.BrandName).MaximumLength(200);
        RuleFor(x => x.Website).Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Invalid website URL");
    }
}
