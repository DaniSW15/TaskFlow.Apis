using MediatR;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;
