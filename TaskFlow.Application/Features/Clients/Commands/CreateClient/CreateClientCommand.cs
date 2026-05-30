using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Commands.CreateClient;

public sealed record CreateClientCommand(
    string Name,
    string Email,
    string? Phone,
    string? Company,
    string? Notes) : IRequest<Result<Guid>>;
