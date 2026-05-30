using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Users.Commands.UpdateUserRole;

public sealed class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            return Result.Failure(Error.NotFound);

        user.Role = request.Role;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
