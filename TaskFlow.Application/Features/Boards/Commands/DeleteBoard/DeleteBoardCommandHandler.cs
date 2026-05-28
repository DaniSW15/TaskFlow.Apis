using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Boards.Commands.DeleteBoard;

public sealed class DeleteBoardCommandHandler : IRequestHandler<DeleteBoardCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBoardCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Boards.GetByIdAsync(request.Id, cancellationToken);

        if (board is null)
            return Result.Failure(Error.NotFound);

        if (board.OwnerId != request.RequestingUserId)
            return Result.Failure(Error.Forbidden);

        _unitOfWork.Boards.Delete(board);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
