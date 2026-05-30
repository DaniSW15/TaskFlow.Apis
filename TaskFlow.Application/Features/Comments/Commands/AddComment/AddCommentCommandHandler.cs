using MediatR;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Comments.Commands.AddComment;

public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskItemId, cancellationToken);
        if (task is null)
            return Result.Failure<Guid>(new Error("Comment.TaskNotFound", "The task does not exist."));

        var comment = new Comment
        {
            Content = request.Content,
            TaskItemId = request.TaskItemId,
            AuthorId = request.AuthorId
        };

        await _unitOfWork.Comments.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(comment.Id);
    }
}
