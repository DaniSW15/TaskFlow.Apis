using MediatR;
using TaskFlow.Application.DTOs.Boards;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Boards.Queries.GetBoardById;

public sealed class GetBoardByIdQueryHandler : IRequestHandler<GetBoardByIdQuery, Result<BoardDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBoardByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BoardDto>> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Boards.GetByIdWithTasksAsync(request.Id, cancellationToken);

        if (board is null)
            return Result.Failure<BoardDto>(Error.NotFound);

        if (board.OwnerId != request.RequestingUserId)
            return Result.Failure<BoardDto>(Error.Forbidden);

        return Result.Success(new BoardDto(
            board.Id,
            board.Title,
            board.Description,
            board.OwnerId,
            board.Tasks.Count,
            board.CreatedAt,
            board.UpdatedAt));
    }
}
