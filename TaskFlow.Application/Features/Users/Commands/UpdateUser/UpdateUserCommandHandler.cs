using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            return Result.Failure(Error.NotFound);

        // Un usuario solo puede editar su propio perfil (o un Admin puede editar cualquiera)
        if (user.Id != request.RequestingUserId)
            return Result.Failure(Error.Forbidden);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
