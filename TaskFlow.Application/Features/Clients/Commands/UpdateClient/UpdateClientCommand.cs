using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Commands.UpdateClient;

public sealed record UpdateClientCommand(
    Guid Id,
    string Name,
    string? Phone,
    string? Company,
    string? Notes) : IRequest<Result>;
