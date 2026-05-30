using MediatR;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Users.Commands.UpdateUserRole;

public sealed record UpdateUserRoleCommand(Guid Id, UserRole Role) : IRequest<Result>;
