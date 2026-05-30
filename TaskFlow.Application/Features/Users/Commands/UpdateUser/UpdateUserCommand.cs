using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    Guid RequestingUserId) : IRequest<Result>;
