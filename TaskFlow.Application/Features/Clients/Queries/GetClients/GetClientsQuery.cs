using MediatR;
using TaskFlow.Application.DTOs.Clients;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Queries.GetClients;

public sealed record GetClientsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<ClientDto>>>;
