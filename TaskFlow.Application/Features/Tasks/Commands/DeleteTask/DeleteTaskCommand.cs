using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tasks.Commands.DeleteTask;

public sealed record DeleteTaskCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;
