using MediatR;
using TaskFlow.Application.DTOs.Clients;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Queries.GetClients;

public sealed class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, Result<PaginatedList<ClientDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClientsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<ClientDto>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var (clients, total) = await _unitOfWork.Clients.GetAllPagedAsync(
            request.PageNumber, request.PageSize, cancellationToken);

        var dtos = clients.Select(c => new ClientDto(
            c.Id,
            c.Name,
            c.Email,
            c.Phone,
            c.Company,
            c.Notes,
            c.Projects.Count,
            c.CreatedAt,
            c.UpdatedAt)).ToList();

        return Result.Success(new PaginatedList<ClientDto>(dtos, total, request.PageNumber, request.PageSize));
    }
}
