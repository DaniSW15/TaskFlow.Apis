using FluentValidation;

namespace TaskFlow.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(200).WithMessage("Project title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required.");

        RuleFor(x => x.AnalystId)
            .NotEmpty().WithMessage("Analyst ID is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
