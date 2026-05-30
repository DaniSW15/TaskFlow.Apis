using MediatR;
using TaskFlow.Application.DTOs.Clients;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Queries.GetClientById;

public sealed record GetClientByIdQuery(Guid Id) : IRequest<Result<ClientDto>>;
