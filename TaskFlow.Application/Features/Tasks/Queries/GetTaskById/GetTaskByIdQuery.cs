using MediatR;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id, Guid RequestingUserId) : IRequest<Result<TaskDto>>;
