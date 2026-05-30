using MediatR;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PaginatedList<UserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (users, total) = await _unitOfWork.Users.GetAllPagedAsync(
            request.PageNumber, request.PageSize, cancellationToken);

        var dtos = users.Select(u => new UserDto(
            u.Id, u.FirstName, u.LastName, u.FullName,
            u.Email, u.Role, u.CreatedAt, u.UpdatedAt)).ToList();

        return Result.Success(new PaginatedList<UserDto>(dtos, total, request.PageNumber, request.PageSize));
    }
}
