using FluentValidation;

namespace TaskFlow.Application.Features.Boards.Commands.CreateBoard;

public sealed class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Board title is required.")
            .MaximumLength(200).WithMessage("Board title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Board description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }
}
