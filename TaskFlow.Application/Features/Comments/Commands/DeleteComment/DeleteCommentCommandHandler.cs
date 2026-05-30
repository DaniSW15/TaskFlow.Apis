using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Comments.Commands.DeleteComment;

public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(request.Id, cancellationToken);

        if (comment is null)
            return Result.Failure(Error.NotFound);

        // Solo el autor o un Admin puede borrar el comentario
        if (comment.AuthorId != request.RequestingUserId)
            return Result.Failure(Error.Forbidden);

        _unitOfWork.Comments.Delete(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
