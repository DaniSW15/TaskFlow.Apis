using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Commands.DeleteClient;

public sealed record DeleteClientCommand(Guid Id) : IRequest<Result>;
