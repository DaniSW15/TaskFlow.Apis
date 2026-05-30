using MediatR;
using TaskFlow.Application.DTOs.Clients;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, Result<ClientDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClientByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClientDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(request.Id, cancellationToken);

        if (client is null)
            return Result.Failure<ClientDto>(Error.NotFound);

        return Result.Success(new ClientDto(
            client.Id,
            client.Name,
            client.Email,
            client.Phone,
            client.Company,
            client.Notes,
            client.Projects.Count,
            client.CreatedAt,
            client.UpdatedAt));
    }
}
