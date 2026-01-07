using FluentValidation;
using Auth.Core.DTOs;

namespace Auth.Core.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit")
            .Matches(@"[@$!%*?&#]").WithMessage("Password must contain at least one special character (@$!%*?&#)");

        RuleFor(x => x.FirstName)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.FirstName))
            .WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.LastName))
            .WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Country)
            .Length(2).When(x => !string.IsNullOrEmpty(x.Country))
            .WithMessage("Country code must be 2 characters (ISO format)");
    }
}
