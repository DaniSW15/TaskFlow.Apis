using MediatR;
using TaskFlow.Application.DTOs.Users;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;
