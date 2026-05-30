using MediatR;
using TaskFlow.Application.Common.Cursors;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTasksByBoardWithCursor;

public sealed class GetTasksByBoardWithCursorQueryHandler
    : IRequestHandler<GetTasksByBoardWithCursorQuery, Result<CursorPaginatedList<TaskDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTasksByBoardWithCursorQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CursorPaginatedList<TaskDto>>> Handle(
        GetTasksByBoardWithCursorQuery request, CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Boards.GetByIdAsync(request.BoardId, cancellationToken);

        if (board is null)
            return Result.Failure<CursorPaginatedList<TaskDto>>(Error.NotFound);

        if (board.OwnerId != request.RequestingUserId)
            return Result.Failure<CursorPaginatedList<TaskDto>>(Error.Forbidden);

        // Decodificar el cursor — si es inválido/nulo, empieza desde el inicio
        var cursor = TaskCursor.Decode(request.Cursor);

        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (tasks, nextCursor) = await _unitOfWork.Tasks.GetByBoardIdWithCursorAsync(
            request.BoardId,
            cursor?.CreatedAt,
            cursor?.Id,
            pageSize,
            cancellationToken);

        var dtos = tasks.Select(t => new TaskDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.DueDate,
            t.BoardId,
            t.AssigneeId,
            t.Assignee?.FullName,
            t.CreatedAt,
            t.UpdatedAt)).ToList();

        return Result.Success(new CursorPaginatedList<TaskDto>(dtos, nextCursor, pageSize));
    }
}
