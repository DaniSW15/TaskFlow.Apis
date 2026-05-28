using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role);
