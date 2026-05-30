using MediatR;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            return Result.Failure<UserDto>(Error.NotFound);

        return Result.Success(new UserDto(
            user.Id, user.FirstName, user.LastName, user.FullName,
            user.Email, user.Role, user.CreatedAt, user.UpdatedAt));
    }
}
