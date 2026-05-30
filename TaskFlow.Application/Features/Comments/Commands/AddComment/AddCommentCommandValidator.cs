using FluentValidation;

namespace TaskFlow.Application.Features.Comments.Commands.AddComment;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.TaskItemId)
            .NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(4000).WithMessage("Comment must not exceed 4000 characters.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author ID is required.");
    }
}
