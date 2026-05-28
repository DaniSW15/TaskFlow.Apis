using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Boards.Commands.UpdateBoard;

public sealed class UpdateBoardCommandHandler : IRequestHandler<UpdateBoardCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBoardCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Boards.GetByIdAsync(request.Id, cancellationToken);

        if (board is null)
            return Result.Failure(Error.NotFound);

        if (board.OwnerId != request.RequestingUserId)
            return Result.Failure(Error.Forbidden);

        board.Title = request.Title;
        board.Description = request.Description;
        board.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Boards.Update(board);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
