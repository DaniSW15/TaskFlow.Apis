using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Clients.Commands.DeleteClient;

public sealed class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteClientCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(request.Id, cancellationToken);

        if (client is null)
            return Result.Failure(Error.NotFound);

        _unitOfWork.Clients.Delete(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
