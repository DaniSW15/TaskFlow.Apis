using MediatR;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Commands.CreateClient;

public sealed class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateClientCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Clients.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(new Error("Client.EmailTaken", "A client with this email already exists."));

        var client = new Client
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Notes = request.Notes
        };

        await _unitOfWork.Clients.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(client.Id);
    }
}
