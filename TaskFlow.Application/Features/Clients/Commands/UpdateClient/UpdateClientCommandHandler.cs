using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Commands.UpdateClient;

public sealed class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClientCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(request.Id, cancellationToken);

        if (client is null)
            return Result.Failure(Error.NotFound);

        client.Name = request.Name;
        client.Phone = request.Phone;
        client.Company = request.Company;
        client.Notes = request.Notes;

        _unitOfWork.Clients.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
