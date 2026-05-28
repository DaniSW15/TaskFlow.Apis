using MediatR;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Result<AuthResponse>>;
