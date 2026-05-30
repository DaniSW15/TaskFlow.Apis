using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Commands.DeleteProject;

public sealed record DeleteProjectCommand(Guid Id) : IRequest<Result>;
