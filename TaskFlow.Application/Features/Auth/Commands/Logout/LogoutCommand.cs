using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(Guid UserId) : IRequest<Result>;
