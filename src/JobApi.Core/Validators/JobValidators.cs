using FluentValidation;
using JobApi.Core.DTOs;

namespace JobApi.Core.Validators;

public class JobRequestValidator : AbstractValidator<JobRequest>
{
    public JobRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Department).NotEmpty();
        RuleFor(x => x.JobType).NotEmpty();
        RuleFor(x => x.Location).NotEmpty();
        RuleFor(x => x.ApplicationDeadline).GreaterThan(DateTime.UtcNow);
    }
}

public class ApplicationRequestValidator : AbstractValidator<ApplicationRequest>
{
    public ApplicationRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Resume).NotNull();
    }
}
