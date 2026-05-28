using FluentValidation;

namespace TaskFlow.Application.Features.Boards.Commands.UpdateBoard;

public sealed class UpdateBoardCommandValidator : AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Board ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Board title is required.")
            .MaximumLength(200).WithMessage("Board title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Board description must not exceed 1000 characters.")
            .When(x => x.Description is not null);
    }
}
