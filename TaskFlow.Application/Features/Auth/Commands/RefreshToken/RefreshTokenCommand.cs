using MediatR;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AuthResponse>>;
