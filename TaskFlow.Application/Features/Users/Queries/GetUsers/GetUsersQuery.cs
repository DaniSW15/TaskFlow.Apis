using MediatR;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedList<UserDto>>>;
