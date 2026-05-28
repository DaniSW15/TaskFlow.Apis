using MediatR;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTaskByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.Id, cancellationToken);

        if (task is null)
            return Result.Failure<TaskDto>(Error.NotFound);

        var board = await _unitOfWork.Boards.GetByIdAsync(task.BoardId, cancellationToken);

        if (board is null)
            return Result.Failure<TaskDto>(Error.NotFound);

        var isOwner = board.OwnerId == request.RequestingUserId;
        var isAssignee = task.AssigneeId == request.RequestingUserId;

        if (!isOwner && !isAssignee)
            return Result.Failure<TaskDto>(Error.Forbidden);

        return Result.Success(new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.BoardId,
            task.AssigneeId,
            task.Assignee?.FullName,
            task.CreatedAt,
            task.UpdatedAt));
    }
}
