using MediatR;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid BoardId,
    Guid? AssigneeId,
    Guid RequestingUserId) : IRequest<Result<Guid>>;
