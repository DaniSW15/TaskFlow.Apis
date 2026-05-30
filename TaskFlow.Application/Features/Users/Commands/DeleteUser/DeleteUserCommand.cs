using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid Id) : IRequest<Result>;
