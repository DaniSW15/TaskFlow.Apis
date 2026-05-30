using FluentValidation;

namespace TaskFlow.Application.Features.Tags.Commands.CreateTag;

public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required.")
            .MaximumLength(50).WithMessage("Tag name must not exceed 50 characters.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Tag color is required.")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex code (e.g. #FF5733).");
    }
}
